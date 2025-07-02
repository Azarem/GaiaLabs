using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System.Globalization;
using System.Text;

namespace GaiaLib.Rom.Rebuild;

public class RomWriter : IDisposable
{
    internal readonly ProjectRoot _projectRoot;
    internal readonly DbRoot _dbRoot;
    public readonly string RomPath;
    public string? BpsPath;
    private FileStream? OutStream;

    public RomWriter(ProjectRoot projectRoot)
    {
        _projectRoot = projectRoot;
        //Load database
        _dbRoot = DbRoot.FromFolder(projectRoot.DatabasePath, projectRoot.SystemPath);
        //Use paths from project if available
        _dbRoot.Paths = projectRoot.Resources ?? _dbRoot.Paths;
        RomPath = Path.Combine(projectRoot.BaseDir, $"{projectRoot.Name}.smc");
    }

    public void Repack()
    {
        using var outStream = File.Create(RomPath);
        {
            //Expand ROM
            outStream.SetLength(0x400000);
            outStream.Position = 0;

            OutStream = outStream;

            var processor = new RomProcessor(this);
            processor.Repack();

            WriteHeader();
            OutStream = null;

            //Ensure the stream is flushed instead of waiting for GC queue
            outStream.Flush();
        }

        //Generate patch after all resources have been closed
        GeneratePatch();
    }

    private void WriteHeader()
    {
        //Maker/game code
        OutStream.Position = 0xFFB0;
        OutStream.Write(Encoding.ASCII.GetBytes("01JG  "));
        for (int i = 0; i < 10; i++)
            OutStream.WriteByte(0);

        //Cartridge Title
        OutStream.Write(Encoding.ASCII.GetBytes(_projectRoot.Name.ToUpper().PadRight(21)));

        //ROM speed and map mode
        OutStream.WriteByte(0x31);

        //Chipset
        OutStream.WriteByte(0x02);

        //ROM Size
        OutStream.WriteByte(0x0C);

        //RAM Size
        OutStream.WriteByte(0x03);

        //Country ID
        OutStream.WriteByte(0x01);

        //Developer ID
        OutStream.WriteByte(0x33);

        //Version
        OutStream.WriteByte(0x00);

        //Calculate checksum
        int sum = 0;
        OutStream.Position = 0;
        for (uint x = 0, size = (uint)OutStream.Length; x++ < size;)
            sum += OutStream.ReadByte();

        //Write checksum
        OutStream.Position = 0xFFDEu;
        OutStream.WriteByte((byte)sum);
        OutStream.WriteByte((byte)(sum >> 8));

        //Write checksum compliment
        sum = ~sum;
        OutStream.Position = 0xFFDCu;
        OutStream.WriteByte((byte)sum);
        OutStream.WriteByte((byte)(sum >> 8));
    }

    private void GeneratePatch()
    {
        if (string.IsNullOrWhiteSpace(_projectRoot.FlipsPath) || !File.Exists(_projectRoot.FlipsPath))
            return;

        BpsPath = Path.Combine(_projectRoot.BaseDir, $"{_projectRoot.Name}.bps");

        using var process = new System.Diagnostics.Process()
        {
            StartInfo = new(_projectRoot.FlipsPath, $"--create --bps \"{_projectRoot.RomPath}\" \"{RomPath}\" \"{BpsPath}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        var result = process.StandardOutput.ReadToEnd();
        Console.WriteLine(result);
    }

    internal void WriteTransform(int location, object value)
    {
        OutStream.Position = location;
        if (value is ushort us)
        {
            OutStream.WriteByte((byte)us);
            OutStream.WriteByte((byte)(us >> 8));
        }
        else if (value is uint ui)
        {
            OutStream.WriteByte((byte)ui);
            OutStream.WriteByte((byte)(ui >> 8));
            OutStream.WriteByte((byte)(ui >> 16));
        }
        else if (value is byte b)
            OutStream.WriteByte(b);
    }

    internal int WriteFile(ChunkFile file, DbRoot root, IDictionary<string, int> chunkLookup)
    {
        int filePos = file.Location;
        OutStream.Position = filePos;

        if (file.Blocks != null)
        {
            //Rebase assembly
            if (file.Blocks?.Any() == true && file.Blocks[0].Location != file.Location && file.Location != 0u)
                throw new("Assembly was not based properly");

            ParseAssembly(file.Blocks, file.Includes, root, chunkLookup, file.IncludeLookup);
        }
        else
        {
            //Open source file
            using var inFile = File.OpenRead(file.Path);
            var remain = file.Size;

            switch (file.Type)
            {
                case BinType.Tilemap:
                    remain -= 2;
                    OutStream.WriteByte((byte)inFile.ReadByte());
                    OutStream.WriteByte((byte)inFile.ReadByte());
                    break;

                case BinType.Meta17:
                    remain -= 4;
                    OutStream.WriteByte((byte)inFile.ReadByte());
                    OutStream.WriteByte((byte)inFile.ReadByte());
                    OutStream.WriteByte((byte)inFile.ReadByte());
                    OutStream.WriteByte((byte)inFile.ReadByte());
                    break;

                case BinType.Sound:
                    remain -= 2;
                    OutStream.WriteByte((byte)remain);
                    OutStream.WriteByte((byte)(remain >> 8));
                    break;

            }

            //Mark as not compressed
            if (file.Compressed != null)
            {
                remain -= 2;
                int inverse = 0 - remain;
                //OutStream.WriteByte(0);
                //OutStream.WriteByte(0);
                OutStream.WriteByte((byte)inverse);
                OutStream.WriteByte((byte)(inverse >> 8));
            }

            //Copy file to rom
            while (remain-- > 0)
                OutStream.WriteByte((byte)inFile.ReadByte());
        }


        int size = (int)OutStream.Position - filePos;

        //Write reference fixups
        var nextPos = OutStream.Position;

        return size;
    }

    private void ParseAssembly(IEnumerable<AsmBlock> blocks, HashSet<string> includes, DbRoot root, IDictionary<string, int> chunkLookup, Dictionary<string, AsmBlock> includeLookup)
    {
        if (blocks == null)
            throw new("Assembly has not been parsed");

        int bix = 0;

        foreach (var block in blocks)
        {
            uint? oldPos = null;
            //if (block.Label == null)
            if (block.Location != OutStream.Position)
            {
                oldPos = (uint)OutStream.Position;
                OutStream.Position = block.Location;
            }
            //else if (block.Location != OutStream.Position)
            //    throw new("Location does not match");
            //block.Location = (uint)OutStream.Position;


            var objList = block.ObjList;
            int oix = 0, opos = 0;


            void ProcessObject(object obj, Op? parentOp = null)
            {
            Top:
                if (obj is Op op)
                {
                    OutStream.WriteByte((byte)op.Code.Code);
                    opos += op.Size;

                    foreach (var opnd in op.Operands)
                        ProcessObject(opnd, op);
                }
                else if (obj is byte[] arr)
                {
                    OutStream.Write(arr);
                    opos += arr.Length;
                }
                //else if (obj is StringEntry sw)
                //{
                //    obj = sw.Data;
                //    goto Top;
                //}
                else if (obj is string str)
                {
                    var label = str;

                    var ix = 0;
                    while (RomProcessingConstants.AddressSpace.Contains(label[ix]))
                        ix++;

                    if (ix > 0)
                        label = label[ix..];

                    int loc;
                    bool isRelative = parentOp != null &&
                        (parentOp.Code.Mode == AddressingMode.PCRelative || parentOp.Code.Mode == AddressingMode.PCRelativeLong);

                    var mathIx = label.IndexOfAny(RomProcessingConstants.Operators);
                    int? offset = null;
                    bool useMarker = false;
                    if (mathIx > 0)
                    {
                        if (label[mathIx + 1] == 'M')
                            useMarker = true;
                        else
                        {
                            offset = int.Parse(label[(mathIx + 1)..], NumberStyles.HexNumber);
                            if (label[mathIx] == '-')
                                offset = -offset;
                        }
                        label = label[..mathIx];
                    }

                    //Search local labels first
                    //var target = blocks.FirstOrDefault(x => label.Equals(x.Label, StringComparison.CurrentCultureIgnoreCase));
                    if (includeLookup.TryGetValue(label.ToUpper(), out var target))
                        loc = target.Location;
                    else if (!chunkLookup.TryGetValue(label.ToUpper(), out loc))
                    //&& includeLookup?.TryGetValue(label.ToUpper(), out target) != true)
                    {
                        //These now don't happen
                        if (label[0] == '#')
                            label = label[1..];

                        if (label[0] == '$')
                            label = label[1..];

                        if (isRelative && label.Length > 4)
                        {
                            var off = int.Parse(label, NumberStyles.HexNumber) - (block.Location + opos);
                            obj = parentOp.Size == 2 ? (object)(byte)off : (ushort)off;
                        }
                        else
                            obj = label.Length switch
                            {
                                1 or 2 => (object)byte.Parse(label, NumberStyles.HexNumber),
                                3 or 4 => ushort.Parse(label, NumberStyles.HexNumber),
                                5 or 6 => uint.Parse(label, NumberStyles.HexNumber),
                                _ => throw new($"Invalid operand '{label}'")
                            };
                        goto Top;
                    }

                    var type = Address.TypeFromCode(str[0]);
                    if (type == AddressType.Unknown)
                        type = parentOp?.Size == 4 ? AddressType.Address
                            : parentOp?.Size == 2 ? AddressType.Unknown : AddressType.Offset;

                    if (isRelative)
                    {
                        loc -= block.Location + opos;
                        if (type == AddressType.Unknown && !(loc < 0x80 || loc >= 0x3FFF80))
                            throw new("Relative out of range");
                    }

                    if (offset != null)
                        loc += offset.Value;
                    else if (useMarker) //block.IsString && target?.IsString == true)
                    {
                        int markerOffset = 0;
                        foreach (var part in target.ObjList)
                        {
                            if (part is StringMarker sm)
                            {
                                loc += markerOffset;
                                break;
                            }
                            else
                                markerOffset += RomProcessingConstants.GetSize(part);
                        }
                    }

                    obj = type switch
                    {
                        AddressType.Offset => (ushort)loc,
                        AddressType.Bank => (byte)(loc >> 16 | ((ushort)loc >= 0x8000 ? 0x80 : 0xC0)),
                        AddressType.WBank => (ushort)(loc >> 16 | ((ushort)loc >= 0x8000 ? 0x80 : 0xC0)),
                        AddressType.Address => loc | ((ushort)loc >= 0x8000 ? 0x800000 : 0xC00000),
                        _ => (object)(byte)loc
                    };

                    //if (str[0] == '&' || parentOp?.Size == 3)
                    //    obj = (ushort)loc.Offset;
                    //else if (str[0] == '^')
                    //    obj = (byte)((loc.Offset >> 16) | ((ushort)loc.Offset >= 0x8000 ? 0x80u : 0xC0u));
                    //else if (str[0] == '*')
                    //    obj = (ushort)((loc.Offset >> 16) | ((ushort)loc.Offset >= 0x8000 ? 0x80u : 0xC0u));
                    ////obj = (byte)((loc.Offset >> 16) | 0xC0);
                    //else if (str[0] == '@')
                    //    obj = loc.Offset | 0xC00000u;
                    //else if (str[0] == '%' || parentOp?.Size == 4)
                    //    obj = loc.Offset | ((ushort)loc.Offset >= 0x8000 ? 0x800000u : 0xC00000u);
                    //else if (parentOp?.Size == 2)
                    //    obj = (byte)loc.Offset;
                    //else
                    //    throw new("Unable to determine operand transform");

                    goto Top;
                }
                else if (obj is uint ui)
                {
                    OutStream.WriteByte((byte)ui);
                    OutStream.WriteByte((byte)(ui >> 8));
                    OutStream.WriteByte((byte)(ui >> 16));
                    //opos += 3;
                }
                else if (obj is int i)
                {
                    OutStream.WriteByte((byte)i);
                    OutStream.WriteByte((byte)(i >> 8));
                    OutStream.WriteByte((byte)(i >> 16));
                    //opos += 3;
                }
                else if (obj is ushort us)
                {
                    OutStream.WriteByte((byte)us);
                    OutStream.WriteByte((byte)(us >> 8));
                    //opos += 2;
                }
                else if (obj is byte b)
                {
                    OutStream.WriteByte(b);
                    //opos += 1;
                }
                else if (obj is StringMarker sm)
                { }
                else
                    throw new($"Unable to process '{obj}'");
            }

            foreach (var obj in objList)
            {
                oix++;
                ProcessObject(obj);
            }

            if (oldPos != null)
                OutStream.Position = oldPos.Value;

            bix++;
        }
    }

    public void Dispose()
    {
        if (OutStream != null)
        {
            OutStream.Dispose();
            OutStream = null;
        }
    }
}
