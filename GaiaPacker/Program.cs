using GaiaLib;
using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Rom;
using GaiaPacker;
using System.Globalization;
using System.Text.Json;

//char[]
//    _whitespace = [' ', '\t'],
//    _operators = ['-', '+'],
//    _commaspace = [',', ' ', '\t'],
//    _addressspace = ['@', '&', '^'],
//    _objectspace = ['<', '['];

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

    baseDir = string.IsNullOrWhiteSpace(project.BaseDir) ? Directory.GetParent(file.Name).FullName : project.BaseDir;
}

var databasePath = Path.Combine(baseDir, "database.json");
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
outRom.Position = 0x0DAFFEu;
outRom.WriteByte(0x00);

//Fix for music load speed (bypass long code sequence)
outRom.Position = 0x0281C9u;
outRom.WriteByte(0x6B);

//Modify SPC program (force command $F0 to always process regardless of data state)
outRom.Position = 0x02944Cu;
outRom.WriteByte(0x00);
outRom.WriteByte(0x00);

//Debugman
//foreach (var loc in DebugmanEntries)
//    ApplyData(loc, DebugmanActor);

Process.Repack(baseDir, databasePath, WriteFile);


uint WriteFile(Process.ChunkFile file, DbRoot root, IDictionary<string, Location> chunkLookup)
{
    uint filePos = file.Location;
    outRom.Position = filePos;

    //Open source file
    using (var inFile = File.OpenRead(file.Path))
    {
        switch (file.File.Type)
        {
            case GaiaLib.Database.BinType.Tilemap:
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                break;

            case GaiaLib.Database.BinType.Meta17:
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                outRom.WriteByte((byte)inFile.ReadByte());
                break;

            case GaiaLib.Database.BinType.Sound:
                var s = file.Size - 2;
                outRom.WriteByte((byte)s);
                outRom.WriteByte((byte)(s >> 8));
                break;

            case GaiaLib.Database.BinType.Assembly:
                ParseAssembly(root, inFile, chunkLookup);
                goto Next;
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


Next:
    uint size = (uint)outRom.Position - filePos;

    //Write reference fixups
    //var nextPos = outRom.Position;
    if (file.File?.XRef?.Any() == true)
        foreach (var rf in file.File.XRef)
        {
            //var offset = uint.Parse(rf, NumberStyles.HexNumber);
            outRom.Position = rf;
            outRom.WriteByte((byte)filePos);
            outRom.WriteByte((byte)(filePos >> 8));
            outRom.WriteByte((byte)((filePos >> 16) + 0xC0));
        }

    //Move to next file
    //outRom.Position = nextPos;
    return size;
}


void ParseAssembly(DbRoot root, Stream inStream, IDictionary<string, Location> chunkLookup)
{

    var blocks = Process.ParseAssembly(root, inStream, (uint)outRom.Position, chunkLookup);
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
        foreach (var obj in objList)
        {
            oix++;
            if (obj is Op op)
            {
                outRom.WriteByte(op.Code.Code);
                opos += op.Size;

                if (op.Code.Mode == AddressingMode.PCRelative || op.Code.Mode == AddressingMode.PCRelativeLong)
                {
                    var label = (string)op.Operands[0];
                    var isLong = op.Code.Mode == AddressingMode.PCRelativeLong;

                    var isImm = label.StartsWith('#');
                    if (isImm)
                        label = label[1..];

                    if (label.StartsWith('$'))
                    {
                        isImm = true;
                        label = label[1..];
                    }

                    Location? offset = null;
                    if (isImm)
                        if (label.Length > 4)
                            offset = uint.Parse(label, NumberStyles.HexNumber);
                        else if (label.Length > 2 && !isLong)
                            throw new($"Invalid operand size for instruction '{op.Code.Mnem}'");
                        else
                            op.Operands[0] = label;
                    else
                    {
                        var target = blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
                            ?? throw new($"Unable to find label {label}");

                        offset = target.Location;
                    }

                    if (offset != null)
                    {
                        offset = offset.Value.Offset - (block.Location.Offset + (uint)opos);
                        op.Operands[0] = isLong ? (ushort)offset.Value.Offset : (object)(byte)offset.Value.Offset;
                    }
                }
                else if (op.Code.Mnem[0] == 'J'
                    && (op.Code.Mode == AddressingMode.Absolute || op.Code.Mode == AddressingMode.AbsoluteLong))
                {
                    var label = (string)op.Operands[0];
                    var isLong = op.Code.Mode == AddressingMode.AbsoluteLong;
                    if (label.StartsWith('@'))
                    {
                        label = label[1..];
                        var target = blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
                            ?? throw new($"Unable to find label {label}");
                        op.Operands[0] = isLong ? (target.Location.Offset + 0x800000u) : (object)(ushort)target.Location.Offset;
                    }
                }

                foreach (var opnd in op.Operands)
                {
                    object result;

                    if (opnd is string str)
                        result = (str.Length) switch
                        {
                            1 or 2 => result = byte.Parse(str, NumberStyles.HexNumber),
                            3 or 4 => result = ushort.Parse(str, NumberStyles.HexNumber),
                            5 or 6 => result = uint.Parse(str, NumberStyles.HexNumber),
                            _ => throw new($"Incorrect operand length {str}")
                        };
                    else
                        result = opnd;


                    if (result is uint ui)
                    {
                        outRom.WriteByte((byte)ui);
                        outRom.WriteByte((byte)(ui >> 8));
                        outRom.WriteByte((byte)(ui >> 16));
                    }
                    else if (result is ushort us)
                    {
                        outRom.WriteByte((byte)us);
                        outRom.WriteByte((byte)(us >> 8));
                    }
                    else if (result is byte b)
                        outRom.WriteByte(b);
                }

            }
            else if (obj is byte[] arr)
            {
                outRom.Write(arr);
                opos += arr.Length;
            }
            else
                throw new($"Unable to process '{obj}'");
        }

        if (oldPos != null)
            outRom.Position = oldPos.Value;

        bix++;
    }
}

