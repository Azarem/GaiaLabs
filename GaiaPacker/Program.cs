using GaiaLib;
using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Rom;
using GaiaPacker;
using System.Globalization;
using System.Text.Json;

char[]
    _whitespace = [' ', '\t'],
    _operators = ['-', '+'],
    _commaspace = [',', ' ', '\t'],
    _addressspace = ['@', '&', '^', '%', '*'],
    _objectspace = ['<', '['];

//uint[] DebugmanEntries = [0x0C82FDu, 0x0CD410u, 0x0CBE7Du, 0x0C9655u];
//byte[] DebugmanActor = [0x20, 0xEE, 0x8B];

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


ProjectRoot project;
var options = new JsonSerializerOptions()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip
};

var baseDir = Environment.CurrentDirectory;
using (var file = File.OpenRead(path))
{
    project = JsonSerializer.Deserialize<ProjectRoot>(file, options)
        ?? throw new("Error deserializing project file");

    //#if DEBUG
    //    baseDir = "C:\\Games\\Dump";
    //#else
    baseDir = string.IsNullOrWhiteSpace(project.BaseDir) ? Directory.GetParent(file.Name).FullName : project.BaseDir;
    //#endif
}

//#if DEBUG
//var databasePath = "C:\\Games\\GaiaLabs\\GaiaLib\\database.json";// Path.Combine(baseDir, "database.json");
//#else
var databasePath = Path.Combine(baseDir, "database.json");
//#endif
//var filePath = Path.Combine(baseDir, "files");

if (isUnpack)
{
    using (var reader = new RomReader(project.RomPath, databasePath))
        reader.DumpDatabase(baseDir);
    return;
}

using var outRom = File.Create(Path.Combine(baseDir, $"{project.Name}.smc"));

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

////Sky Deliveryman!
//outRom.Position = 0x0CB49Fu;
//outRom.WriteByte(0x47);

//Fix for Meta17 overflow (last byte is not used)
//outRom.Position = 0x0DAFFEu;
//outRom.WriteByte(0x00);

//Fix for music load speed (bypass long code sequence)
//outRom.Position = 0x0281C9u;
//outRom.WriteByte(0x6B);

//outRom.Position = 0x028B91u;
//for (int i = 0; i < 6; i++)
//    outRom.WriteByte(0xEA);

//Modify SPC program (force command $F0 to always process regardless of data state)
//outRom.Position = 0x02944Cu;
//outRom.WriteByte(0x00);
//outRom.WriteByte(0x00);

//Debugman
//foreach (var loc in DebugmanEntries)
//    ApplyData(loc, DebugmanActor);

Process.Repack(baseDir, databasePath, WriteFile, WriteTransform);

//Calculate checksum
int sum = 0;
outRom.Position = 0;
for (uint x = 0, size = (uint)outRom.Length; x++ < size;)
    //while (outRom.Position < outRom.Length)
    sum += outRom.ReadByte();// | (outRom.ReadByte() << 8);

//Write checksum
outRom.Position = 0xFFDEu;
outRom.WriteByte((byte)sum);
outRom.WriteByte((byte)(sum >> 8));

//Write checksum compliment
sum = ~sum;
outRom.Position = 0xFFDCu;
outRom.WriteByte((byte)sum);
outRom.WriteByte((byte)(sum >> 8));

void WriteTransform(uint location, object value)
{
    return;
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


    if (file.File.Type == BinType.Assembly)
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

        switch (file.File.Type)
        {
            case BinType.Tilemap:
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                break;

            case BinType.Meta17:
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                break;

            case BinType.Sound:
                var s = file.Size - 2;
                outRom.WriteByte((byte)s);
                outRom.WriteByte((byte)(s >> 8));
                break;

        }

        //Mark as not compressed
        if (file.File.Compressed != null)
        {
            int inverse = 0 - (file.Size - 2);
            //outRom.WriteByte(0);
            //outRom.WriteByte(0);
            outRom.WriteByte((byte)inverse);
            outRom.WriteByte((byte)(inverse >> 8));
        }

        //Copy file to rom
        int sample;
        while ((sample = inFile.ReadByte()) >= 0)
            outRom.WriteByte((byte)sample);
    }


    uint size = (uint)outRom.Position - filePos;

    //Write reference fixups
    var nextPos = outRom.Position;

    //if (file.File.XRef != null)
    //{
    //    foreach (var str in file.File.XRef)
    //    {
    //        char c = str[0];
    //        var hex = _addressspace.Contains(c) ? str[1..] : str;
    //        var targetPos = filePos;

    //        var opIx = hex.IndexOfAny(['-', '+']);
    //        if (opIx > 0)
    //        {
    //            var num = uint.Parse(hex[(opIx + 1)..], NumberStyles.HexNumber);
    //            if (hex[opIx] == '-')
    //                targetPos -= num;
    //            else
    //                targetPos += num;
    //            hex = hex[..opIx];
    //        }

    //        var value = uint.Parse(hex, NumberStyles.HexNumber);
    //        Location loc = value;

    //        outRom.Position = loc;
    //        switch (c)
    //        {
    //            case '@':
    //                value = targetPos | 0xC00000u;
    //                goto writeValue;

    //            case '%':
    //                value = targetPos | 0x800000u;
    //                goto writeValue;

    //            case '^':
    //                outRom.WriteByte((byte)((targetPos >> 16) | 0x80));
    //                break;

    //            case '*':
    //                outRom.WriteByte((byte)((targetPos >> 16) | 0xC0));
    //                break;

    //            case '&':
    //                outRom.WriteByte((byte)targetPos);
    //                outRom.WriteByte((byte)(targetPos >> 8));
    //                break;

    //            default:
    //                value = (value & 0xC00000) | targetPos;

    //            writeValue:
    //                outRom.WriteByte((byte)value);
    //                outRom.WriteByte((byte)(value >> 8));
    //                outRom.WriteByte((byte)(value >> 16));
    //                break;
    //        }
    //    }
    //}

    return size;
}


void ParseAssembly(IEnumerable<AsmBlock> blocks, HashSet<string> includes, DbRoot root, IDictionary<string, Location> chunkLookup, IDictionary<string, Location> includeLookup)
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

                var mathIx = label.IndexOfAny(_operators);
                int? offset = null;
                if (mathIx > 0)
                {
                    offset = int.Parse(label[(mathIx + 1)..], NumberStyles.HexNumber);
                    if (label[mathIx] == '-')
                        offset = -offset;
                    label = label[..mathIx];
                }

                //Search local labels first
                var target = blocks.FirstOrDefault(x => x.Label?.Equals(label, StringComparison.CurrentCultureIgnoreCase) == true);
                if (target != null)
                    loc = target.Location;
                else if (!chunkLookup.TryGetValue(label.ToUpper(), out loc)
                    && includeLookup?.TryGetValue(label.ToUpper(), out loc) != true)
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

                if (isRelative)
                    loc -= block.Location.Offset + (uint)opos;

                if (offset != null)
                    loc = (uint)((int)loc.Offset + offset.Value);

                var type = Address.TypeFromCode(str[0]);
                if (type == AddressType.Unknown)
                    type = parentOp?.Size == 4 ? AddressType.Address
                        : parentOp?.Size == 2 ? AddressType.Unknown : AddressType.Offset;

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

