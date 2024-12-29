using GaiaLib;
using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Rom;
using System.Globalization;

string? path = "project.json";
var isUnpack = false;
foreach (var a in args)
{
    if (a.StartsWith("--"))
    {
        var b = a[2..].ToLower();
        switch (b)
        {
            case "unpack":
                isUnpack = true;
                break;
        }
    }
    else
        path = a;
}

ProjectRoot project = ProjectRoot.Load();

if (isUnpack)
{
    using (var reader = new RomReader(project.RomPath, project.DatabasePath))
        reader.DumpDatabase(project.BaseDir);
    return;
}

var outPath = Path.Combine(project.BaseDir, $"{project.Name}.smc");
using var outRom = File.Create(outPath);

using (var rom = File.OpenRead(project.RomPath))
{
    int sample;
    while ((sample = rom.ReadByte()) >= 0)
        outRom.WriteByte((byte)sample);
}

//Expand ROM
while (outRom.Position < 0x400000)
    outRom.WriteByte(0);

//Update size in header
outRom.Position = 0xFFD7;
outRom.WriteByte(0x0C);

//Repack
Process.Repack(project.BaseDir, project.DatabasePath, WriteFile, WriteTransform);

//Calculate checksum
int sum = 0;
outRom.Position = 0;
for (uint x = 0, size = (uint)outRom.Length; x++ < size;)
    sum += outRom.ReadByte();

//Write checksum
outRom.Position = 0xFFDEu;
outRom.WriteByte((byte)sum);
outRom.WriteByte((byte)(sum >> 8));

//Write checksum compliment
sum = ~sum;
outRom.Position = 0xFFDCu;
outRom.WriteByte((byte)sum);
outRom.WriteByte((byte)(sum >> 8));

outRom.Flush();
outRom.Dispose();

//Generate patch
if (!string.IsNullOrWhiteSpace(project.FlipsPath) && File.Exists(project.FlipsPath))
{
    var bpsPath = Path.Combine(project.BaseDir, $"{project.Name}.bps");

    using var process = new System.Diagnostics.Process()
    {
        StartInfo = new(project.FlipsPath, $"--create --bps \"{project.RomPath}\" \"{outPath}\" \"{bpsPath}\"")
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

void WriteTransform(uint location, object value)
{
    outRom.Position = location;
    if (value is ushort us)
    {
        outRom.WriteByte((byte)us);
        outRom.WriteByte((byte)(us >> 8));
    }
    else if (value is uint ui)
    {
        outRom.WriteByte((byte)ui);
        outRom.WriteByte((byte)(ui >> 8));
        outRom.WriteByte((byte)(ui >> 16));
    }
    else if (value is byte b)
        outRom.WriteByte(b);
}

uint WriteFile(Process.ChunkFile file, DbRoot root, IDictionary<string, Location> chunkLookup)
{
    uint filePos = file.Location;
    outRom.Position = filePos;

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
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                break;

            case BinType.Meta17:
                remain -= 4;
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                break;

            case BinType.Sound:
                remain -= 2;
                outRom.WriteByte((byte)remain);
                outRom.WriteByte((byte)(remain >> 8));
                break;

        }

        //Mark as not compressed
        if (file.Compressed != null)
        {
            remain -= 2;
            int inverse = 0 - remain;
            //outRom.WriteByte(0);
            //outRom.WriteByte(0);
            outRom.WriteByte((byte)inverse);
            outRom.WriteByte((byte)(inverse >> 8));
        }

        //Copy file to rom
        while (remain-- > 0)
            outRom.WriteByte((byte)inFile.ReadByte());
    }


    uint size = (uint)outRom.Position - filePos;

    //Write reference fixups
    var nextPos = outRom.Position;

    return size;
}

void ParseAssembly(IEnumerable<AsmBlock> blocks, HashSet<string> includes, DbRoot root, IDictionary<string, Location> chunkLookup, Dictionary<string, AsmBlock> includeLookup)
{
    if (blocks == null)
        throw new("Assembly has not been parsed");
    //blocks ??= Process.ParseAssembly(root, inStream, (uint)outRom.Position, out includes);


    int bix = 0;

    foreach (var block in blocks)
    {
        uint? oldPos = null;
        //if (block.Label == null)
        if (block.Location != outRom.Position)
        {
            oldPos = (uint)outRom.Position;
            outRom.Position = block.Location;
        }
        //else if (block.Location != outRom.Position)
        //    throw new("Location does not match");
        //block.Location = (uint)outRom.Position;


        var objList = block.ObjList;
        int oix = 0, opos = 0;


        void ProcessObject(object obj, Op? parentOp = null)
        {
        Top:
            if (obj is Op op)
            {
                outRom.WriteByte(op.Code.Code);
                opos += op.Size;

                foreach (var opnd in op.Operands)
                    ProcessObject(opnd, op);
            }
            else if (obj is byte[] arr)
            {
                outRom.Write(arr);
                opos += arr.Length;
            }
            else if (obj is Process.StringEntry sw)
            {
                obj = sw.Data;
                goto Top;
            }
            else if (obj is string str)
            {
                var label = str;

                var ix = 0;
                while (Process._addressspace.Contains(label[ix]))
                    ix++;

                if (ix > 0)
                    label = label[ix..];

                Location loc;
                bool isRelative = parentOp != null &&
                    (parentOp.Code.Mode == AddressingMode.PCRelative || parentOp.Code.Mode == AddressingMode.PCRelativeLong);

                var mathIx = label.IndexOfAny(Process._operators);
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
                        var off = int.Parse(label, NumberStyles.HexNumber) - ((int)block.Location.Offset + opos);
                        obj = parentOp.Size == 2 ? (object)(byte)off : (ushort)off;
                    }
                    else
                        obj = (label.Length) switch
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
                    loc -= block.Location.Offset + (uint)opos;
                    if (type == AddressType.Unknown && !(loc.Offset < 0x80 || loc.Offset >= 0x3FFF80))
                        throw new("Relative out of range");
                }

                if (offset != null)
                    loc = (uint)((int)loc.Offset + offset.Value);
                else if (useMarker) //block.IsString && target?.IsString == true)
                {
                    int markerOffset = 0;
                    foreach (var part in target.ObjList)
                    {
                        if (part is Process.StringMarker sm)
                        {
                            loc += (uint)markerOffset;
                            break;
                        }
                        else
                            markerOffset += Process.GetSize(part);
                    }
                }

                obj = type switch
                {
                    AddressType.Offset => (ushort)loc.Offset,
                    AddressType.Bank => (byte)((loc.Offset >> 16) | ((ushort)loc.Offset >= 0x8000 ? 0x80u : 0xC0u)),
                    AddressType.WBank => (ushort)((loc.Offset >> 16) | ((ushort)loc.Offset >= 0x8000 ? 0x80u : 0xC0u)),
                    AddressType.Address => loc.Offset | ((ushort)loc.Offset >= 0x8000 ? 0x800000u : 0xC00000u),
                    _ => (object)(byte)loc.Offset
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
                outRom.WriteByte((byte)ui);
                outRom.WriteByte((byte)(ui >> 8));
                outRom.WriteByte((byte)(ui >> 16));
                //opos += 3;
            }
            else if (obj is ushort us)
            {
                outRom.WriteByte((byte)us);
                outRom.WriteByte((byte)(us >> 8));
                //opos += 2;
            }
            else if (obj is byte b)
            {
                outRom.WriteByte(b);
                //opos += 1;
            }
            else if (obj is Process.StringMarker sm)
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
            outRom.Position = oldPos.Value;

        bix++;
    }
}
