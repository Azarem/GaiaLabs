using GaiaLib;
using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaPacker;
using System.Globalization;
using System.Text.Json;

char[]
    _whitespace = [' ', '\t'],
    _operators = ['-', '+'],
    _commaspace = [',', ' ', '\t'],
    _addressspace = ['@', '&', '^'],
    _objectspace = ['<', '['];

uint[] DebugmanEntries = [0x0C82FDu, 0x0CD410u, 0x0CBE7Du, 0x0C9655u];
byte[] DebugmanActor = [0x20, 0xEE, 0x8B];

ProjectRoot project;
var options = new JsonSerializerOptions()
{
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip
};

using (var file = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\project.json"))
    project = JsonSerializer.Deserialize<ProjectRoot>(file, options)
        ?? throw new("Error deserializing project file");

var baseDir = project.BaseDir;

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


//Apply COP patches
//outRom.Position = 0x00F48F;
//using (var asmFile = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\Cop51Patch.asm"))
//    ParseAssembly(asmFile);

//Apply meta patches
//outRom.Position = 0x02F08C;
//using (var asmFile = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\TilesetPatch.asm"))
//    ParseAssembly(asmFile);
//using (var asmFile = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\TilemapPatch.asm"))
//    ParseAssembly(asmFile);
//using (var asmFile = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\MenuBGPatch.asm"))
//    ParseAssembly(asmFile);
//using (var asmFile = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\SpritemapPatch.asm"))
//    ParseAssembly(asmFile);
//using (var asmFile = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\BitmapPatch.asm"))
//    ParseAssembly(asmFile);

//Apply sfx load patch
//using (var asmFile = File.OpenRead("C:\\Games\\GaiaLabs\\GaiaPacker\\SFXTable.asm"))
//    ParseAssembly(asmFile);

//Debugman
//foreach (var loc in DebugmanEntries)
//    ApplyData(loc, DebugmanActor);

Process.Repack(baseDir, "C:\\Games\\GaiaLabs\\GaiaLabs\\database.json", WriteFile);

//uint WriteSfx(Location location, IEnumerable<string> paths)
//{
//    var offset = location.Offset;
//    outRom.Position = offset;

//    void writeByte(byte b)
//    {
//        outRom.WriteByte(b);
//        if ((++offset & 0x8000u) != 0)
//        {
//            offset = (offset & 0x3F0000) + 0x10000;// | (offset & 0x7FFFu);
//            outRom.Position = offset;
//        }
//    }

//    foreach (var path in paths)
//    {
//        using var inFile = File.OpenRead(path);

//        writeByte((byte)inFile.Length);
//        writeByte((byte)(inFile.Length >> 8));

//        int sample;
//        while ((sample = inFile.ReadByte()) >= 0)
//            writeByte((byte)sample);
//    }

//    return (uint)outRom.Position;
//}

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

//const int ChunkSize = 0x10000;

//foreach (var entry in project.Files)
//{
//    entry.Path = File.Exists(entry.Path) ? entry.Path : Path.Combine(baseDir, entry.Path);
//    entry.Size = (int)(new FileInfo(entry.Path).Length + 2);
//}

//var remainingFiles = project.Files.OrderByDescending(x => x.Size).ToArray();
//var totalCount = remainingFiles.Length;
//var remainingCount = remainingFiles.Length;

//const int MaxDepth = 0x100;
//var comboBuffer = new int[MaxDepth];
//var bestBuffer = new int[MaxDepth];

//outRom.Position = 0x200000;
//while (remainingCount > 0)
//{
//    var bestRemain = ChunkSize;
//    //var bestCombo = new List<int>();
//    var bestDepth = 0;
//    //var currentCombo = new Stack<int>();
//    int smix = totalCount;
//    while (remainingFiles[--smix] == null) ;

//    var smallest = remainingFiles[smix].Size;

//    bool searchDim(int ix, int depth, int remain)
//    {
//        if (remain < smallest)
//            return false;

//        //while (ix < remainingFiles.Count)
//        for (int i = ix; i < totalCount; i++)
//        {
//            var item = remainingFiles[i];

//            if (item == null || item.Size > remain)
//                continue;

//            var newRemain = remain - item.Size;

//            ////Skip files that overflow
//            //if (newRemain < 0)
//            //    continue;

//            //Add item to combo
//            //currentCombo.Push(i);
//            comboBuffer[depth] = i;

//            //Evaluate against best
//            if (newRemain < bestRemain)
//            {
//                //bestCombo.Clear();
//                //bestCombo.AddRange(currentCombo);
//                Array.Copy(comboBuffer, bestBuffer, depth + 1);
//                bestRemain = newRemain;
//                bestDepth = depth + 1;
//            }

//            if (remain < smallest)
//                return false;

//            //Check for an good match
//            var exact = newRemain < 100 || searchDim(i + 1, depth + 1, newRemain);

//            //Remove current item from combo
//            //currentCombo.Pop();

//            if (exact)
//                return true;

//            //if (bestRemain < smallest)
//            //    break;
//        }

//        return false;
//    }

//    //Find best chunk combination
//    searchDim(0, 0, ChunkSize);

//    //Add files
//    var chunkPos = outRom.Position;
//    for (var i = 0; i < bestDepth; i++)
//    {
//        var ix = bestBuffer[i];
//        var entry = remainingFiles[ix];
//        var filePos = (uint)outRom.Position;

//        //Remove item from list
//        remainingFiles[ix] = null;
//        remainingCount--;

//        //Open source file
//        using (var inFile = File.OpenRead(entry.Path))
//        {
//            switch (entry.Type)
//            {
//                case "Tilemap":
//                    outRom.WriteByte((byte)inFile.ReadByte());
//                    outRom.WriteByte((byte)inFile.ReadByte());
//                    break;

//                case "Assembly":
//                    ParseAssembly(inFile);
//                    continue;
//            }

//            //Mark as not compressed
//            outRom.WriteByte(0);
//            outRom.WriteByte(0);

//            //Copy file to rom
//            int sample;
//            while ((sample = inFile.ReadByte()) >= 0)
//                outRom.WriteByte((byte)sample);
//        }

//        //Write reference fixups
//        var nextPos = outRom.Position;
//        foreach (var rf in entry.Refs)
//        {
//            var offset = uint.Parse(rf, NumberStyles.HexNumber);
//            outRom.Position = offset;
//            outRom.WriteByte((byte)filePos);
//            outRom.WriteByte((byte)(filePos >> 8));
//            outRom.WriteByte((byte)((filePos >> 16) + 0xC0));
//        }

//        //Move to next file
//        outRom.Position = nextPos;
//    }

//    outRom.Position = chunkPos + ChunkSize;

//    Console.WriteLine($"Chunk {chunkPos:X6} written with {bestDepth} files 0x{bestRemain:X} remaining.");
//    ////Remove files that were added
//    //for (var i = bestDepth; i > 0;)
//    //    remainingFiles.RemoveAt(bestBuffer[--i]);
//}

////Apply files
//outRom.Position = 0x210000;
//foreach (var entry in project.Files)
//{
//    //if (entry.Type == "Tilemap")
//    //    continue;
//    var filePos = (uint)outRom.Position;

//    using var inFile = File.OpenRead(File.Exists(entry.Path) ? entry.Path
//        : Path.Combine(baseDir, entry.Path));

//    var fileSize = (uint)inFile.Length;
//    //Test if we would be crossing a bank boundary
//    //if ((filePos + fileSize) >> 16 != filePos >> 16)
//    //{
//    //    filePos += 0x10000 - filePos % 0x10000;
//    //    outRom.Position = filePos;
//    //}

//    switch (entry.Type)
//    {
//        case "Tilemap":
//            outRom.WriteByte((byte)inFile.ReadByte());
//            outRom.WriteByte((byte)inFile.ReadByte());
//            break;

//        case "Assembly":
//            ParseAssembly(inFile);
//            continue;
//    }

//    outRom.WriteByte(0);
//    outRom.WriteByte(0);

//    int sample;
//    while ((sample = inFile.ReadByte()) >= 0)
//        outRom.WriteByte((byte)sample);

//    var nextPos = outRom.Position;
//    foreach (var rf in entry.Refs)
//    {
//        var offset = uint.Parse(rf, NumberStyles.HexNumber);
//        if (offset == 0x0DAF2Cu)
//        {

//        }
//        outRom.Position = offset;
//        outRom.WriteByte((byte)filePos);
//        outRom.WriteByte((byte)(filePos >> 8));
//        outRom.WriteByte((byte)((filePos >> 16) + 0xC0));
//    }

//    outRom.Position = nextPos;
//}

void ApplyData(uint location, byte[] data)
{
    outRom.Position = location;
    outRom.Write(data);
}

void ApplyPatch(uint entry, byte[] asm)
{
    var pos = (uint)outRom.Position;

    outRom.Position = entry;

    if (entry >> 16 == pos >> 16)
    {
        outRom.WriteByte(0x4C);
        outRom.WriteByte((byte)pos);
        outRom.WriteByte((byte)(pos >> 8));
    }
    else
    {
        outRom.WriteByte(0x5C);
        outRom.WriteByte((byte)pos);
        outRom.WriteByte((byte)(pos >> 8));
        outRom.WriteByte((byte)((pos >> 16) + 0x80));
    }

    //outRom.Position = pos;
    //outRom.Write(asm);
    ApplyData(pos, asm);
}

int getSize(object obj)
{
    //var obj = objList[index];
    if (obj is Op op)
        return op.Size;
    else if (obj is byte[] arr)
        return arr.Length;
    else if (obj is byte)
        return 1;
    else if (obj is ushort)
        return 2;
    else if (obj is uint)
        return 3;
    ///TODO: Add parsers for strings
    throw new($"Unable to get size for operand '{obj}'");
}

void ParseAssembly(DbRoot root, Stream inStream, IDictionary<string, Location> chunkLookup)
{
    using var reader = new StreamReader(inStream);

    var blocks = new List<AsmBlock>();
    AsmBlock current;
    AsmBlock? target;
    var tags = new Dictionary<string, string?>();
    int ix, bix = 0, lineCount = 0;

    blocks.Add(current = new AsmBlock { Location = (uint)outRom.Position });

    string? line = "";

    string? getLine()
    {
        //This can happen
        if (line == null)
            return null;

        //Keep processing what we already have
        if (line.Length > 0)
            return line;

        Read:
        line = reader.ReadLine();

        //This can happen
        if (line == null)
            return null;

        lineCount++;

        //Ignore comments
        if ((ix = line.IndexOf("--")) >= 0)
            line = line[..ix];

        //Ignore comments
        if ((ix = line.IndexOf(';')) >= 0)
            line = line[..ix];

        //Trim
        line = line.Trim(_commaspace);

        //This can happen
        if (line.Length == 0)
            goto Read;

        //Make everything case-insensitive
        line = line.ToUpper();

        //Process directives
        if (line[0] == '?')
        {
            ///TODO: Something later
            goto Read;
        }

        //Process tags
        if (line[0] == '!')
        {
            line = line[1..].TrimStart(_commaspace);

            while (line.Length > 0)
            {
                string name = line;
                string? value = null;
                if ((ix = line.IndexOfAny(_commaspace)) >= 0)
                {
                    name = line[..ix];
                    value = line[(ix + 1)..].TrimStart(_commaspace);
                    if ((ix = value.IndexOfAny(_commaspace)) >= 0)
                    {
                        line = value[(ix + 1)..].TrimStart(_commaspace);
                        value = value[..ix];
                    }
                    else
                        line = "";
                }
                else
                    line = "";

                tags[name] = value;
            }

            goto Read;
        }

        //Resolve tags
        foreach (var tag in tags)
            while ((ix = line.IndexOf(tag.Key)) >= 0)
                line = line[..ix] + tag.Value + line[(ix + tag.Key.Length)..];

        //Resolve addresses
        while ((ix = line.IndexOfAny(_addressspace)) >= 0)
        {
            var endIx = line.IndexOfAny(_commaspace, ix);
            if (endIx < 0) endIx = line.Length;
            var label = line[(ix + 1)..endIx];

            if (!chunkLookup.TryGetValue(label, out var loc))
            {
                target = blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
                    ?? throw new($"Unable to find label {label}");
                loc = target.Location;
            }

            var result = (line[ix]) switch
            {
                '@' => (loc.Offset + 0xC00000u).ToString("X6"),
                '&' => (loc.Offset & 0x00FFFFu).ToString("X4"),
                '^' => ((loc.Offset >> 16) + 0xC0).ToString("X2"),
                _ => throw new("Invalid address operator")
            };

            line = line[..ix] + '$' + result + line[endIx..];
        }


        return line;
    }

    void processText(string? struc = null, char? openTag = null)
    {

        DbStruct? dbs = null;
        uint memoff = 0;

        if (struc != null)
            dbs = root.Structs.Values.FirstOrDefault(x => x.Name.Equals(struc, StringComparison.CurrentCultureIgnoreCase));

        var parent = dbs?.Parent != null
            ? root.Structs.Values.FirstOrDefault(x => x.Name.Equals(dbs.Parent, StringComparison.CurrentCultureIgnoreCase))
            : null;

        HexString? desc = parent?.Descriminator;
        bool checkDesc()
        {
            if (desc?.Value == memoff)
            {
                var obj = desc.Value.ToObject();
                current.ObjList.Add(obj);
                current.Size += (uint)getSize(obj);
                return true;
            }
            return false;
        }

        checkDesc();

        while (line != null)
        {
            getLine();
            if (line == null)
                return;

            //Force a stop when we have processed all members of a struct?
            if (memoff >= dbs?.Parts?.Length)
            {
                return;
            }

            if (line.StartsWith("DB"))
            {
                string hex = OpCode.HexRegex().Replace(line[2..], "");
                var data = Convert.FromHexString(hex);
                current.ObjList.Add(data);
                current.Size += (uint)data.Length;
                line = "";
                continue;
            }


            string? mnem = null, operand = null;
            while (line.Length > 0)
            {
                //Process strings
                if (line[0] == '`')
                {
                    string? str = null;
                    if ((ix = line.IndexOf('`', 1)) >= 0)
                    {
                        str = line[1..ix];
                        line = line[ix..];
                    }
                    else
                    {
                        str = line;
                        line = "";
                    }

                    continue;
                }


                //Process raw data
                if (line[0] == '#')
                {
                    bool reverse = false;
                    if (line.StartsWith("#$"))
                    {
                        reverse = true;
                        line = line[2..];
                    }
                    else
                        line = line[1..];

                    string hex;
                    if ((ix = line.IndexOfAny(_commaspace)) >= 0)
                    {
                        hex = line[..ix];
                        line = line[(ix + 1)..].TrimStart(_commaspace);
                    }
                    else
                    {
                        hex = line;
                        line = "";
                    }

                    if (hex.Length > 0)
                    {
                        var data = Convert.FromHexString(hex);
                        if (reverse)
                            for (int x = 0, y = data.Length; --y > x;)
                            {
                                var sample = data[x];
                                data[x++] = data[y];
                                data[y] = sample;
                            }

                        current.ObjList.Add(data);
                        current.Size += (uint)data.Length;
                    }

                    continue;
                }


                if (line[0] == '>')
                {
                    if (openTag == '<')
                        line = line[1..];
                    return;
                }


                if (line[0] == ']')
                {
                    if (openTag == '[')
                        line = line[1..];
                    return;
                }

                //break;


                //Process origin tags
                if (line.StartsWith("ORG"))
                {
                    line = line[3..].TrimStart(_commaspace);
                    if (line.StartsWith('$'))
                        line = line[1..];

                    string hex;
                    if ((ix = line.IndexOfAny(_commaspace)) >= 0)
                    {
                        hex = line[..ix];
                        line = line[(ix + 1)..].TrimStart(_commaspace);
                    }
                    else
                    {
                        hex = line;
                        line = "";
                    }

                    var location = uint.Parse(hex, NumberStyles.HexNumber);

                    blocks.Add(current = new AsmBlock { Location = location });
                    bix++;

                    continue;
                }

                //Process labels
                if ((ix = line.IndexOf(':')) >= 0)
                {
                    var label = line[..ix].TrimEnd(_commaspace);

                    if (label.StartsWith('$'))
                        label = label[1..];

                    if (label.Length == 6
                        && uint.TryParse(label, NumberStyles.HexNumber, null, out var addr)
                        && Location.MaxValue >= addr)
                    {
                        blocks.Add(current = new AsmBlock { Location = addr });
                        bix++;
                    }
                    else if (label.Length > 0)
                    {
                        //if (current.Size == 0 && current.Label == null)
                        //    current.Label = label;
                        //else
                        //{
                        blocks.Add(current = new AsmBlock { Location = current.Location + current.Size, Label = label });
                        bix++;
                        //}
                    }

                    line = line[(ix + 1)..].TrimStart(_commaspace);
                    continue;
                }

                //Separate instructions into mnemonic and operand parts
                if ((ix = line.IndexOfAny(_commaspace)) > 0)
                {
                    mnem = line[..ix];
                    operand = line[(ix + 1)..].TrimStart(_commaspace);

                    //Process object tags
                    if (operand.StartsWith('<'))
                    {
                        line = line[1..].TrimStart(_commaspace);
                        processText(mnem, '<');
                        mnem = null;
                        continue;
                    }
                }
                else
                    mnem = line;

                line = "";
                break;
            }

            if (mnem?.Length > 0)
            {
                //Get list of opcodes from mnemonic
                if (!OpCode.Grouped.TryGetValue(mnem, out var codes))
                    throw new($"Unknown instruction line {lineCount}: '{mnem}'");

                //No operand instructions
                if (string.IsNullOrEmpty(operand))
                {
                    current.ObjList.Add(new Op { Code = codes.First(x => x.Size == 1), Operands = [], Size = 1 });
                    current.Size++;
                    continue;
                }

                //Do maths before regex processing
                while ((ix = operand.IndexOfAny(_operators)) >= 0)
                {
                    var op = operand[ix];

                    int vix = operand.LastIndexOf('$', ix) + 1;
                    //if (vix == 0)
                    //    throw new($"Unable to locate variable for operator line {lineCount}: '{op}'");

                    var value = uint.Parse(operand[vix..ix], NumberStyles.HexNumber);

                    var endIx = operand.IndexOfAny(_commaspace, ix + 1);
                    if (endIx < 0) endIx = operand.Length;
                    var number = uint.Parse(operand[(ix + 1)..endIx], NumberStyles.HexNumber);

                    if (op == '-') value -= number;
                    else value += number;

                    var len = (ix - vix) switch
                    {
                        1 or 2 => 2,
                        3 or 4 => 4,
                        5 or 6 => 6,
                        _ => throw new("Invalid size")
                    };

                    operand = operand[..vix] + value.ToString($"X{len}") + operand[endIx..];
                }

                OpCode opCode = null;
                int size = 1;
                foreach (var code in codes)
                {
                    ///TODO: COP processing
                    //Keep branch operands until all blocks are processed (for labels)
                    if (code.Mode == AddressingMode.PCRelative || code.Mode == AddressingMode.PCRelativeLong)
                    {
                        opCode = code;
                        size = code.Size;
                        break;
                    }

                    if (OpCode.AddressingRegex.TryGetValue(code.Mode, out var regex))
                    {
                        var match = regex.Match(operand);
                        if (match.Success)
                        {
                            operand = match.Groups[1].Value;
                            opCode = code;
                            size = opCode.Size == -2 ? (operand.Length > 2 ? 3 : 2) : opCode.Size;
                            break;
                        }
                    }

                }

                if (opCode == null)
                {
                    if (mnem.StartsWith('J'))
                    {
                        opCode = codes.Single(x => x.Mode == AddressingMode.Absolute || x.Mode == AddressingMode.AbsoluteLong);
                        operand = $"@{operand}";
                        size = opCode.Size;
                        //opnd = blocks.FirstOrDefault(x => x.Label?.Equals(operand) == true)
                        //    //if (!blocks.Any(x => x.Label?.Equals(operand) == true))
                        //    ?? throw new($"Unable to locate target for label line {lineCount}: '{operand}'");
                    }
                    else
                        throw new($"Unable to determine mode/code line {lineCount}: '{line}'");
                }

                current.ObjList.Add(new Op() { Code = opCode, Operands = [operand], Size = (byte)size });
                current.Size += (uint)size;
                //}
            }
        }
    }

    processText(null);

    bix = 0;
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

                    //op.Operands[0] = label.Length switch
                    //{
                    //    2 when byte.TryParse(label, NumberStyles.HexNumber, null, out var b) => b,
                    //    4 when ushort.TryParse(label, NumberStyles.HexNumber, null, out var s) => s,
                    //    6 when uint.TryParse(label, NumberStyles.HexNumber, null, out var i) => i,
                    //    _ => blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
                    //        ?? throw new($"Unable to find label {label}")
                    //};

                    //if (isRelative)
                    //{

                    //}

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
                        target = blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
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
                        target = blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
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
            else if (obj is uint ui)
            {
                outRom.WriteByte((byte)ui);
                outRom.WriteByte((byte)(ui >> 8));
                outRom.WriteByte((byte)(ui >> 16));
                opos += 3;
            }
            else if (obj is ushort us)
            {
                outRom.WriteByte((byte)us);
                outRom.WriteByte((byte)(us >> 8));
                opos += 2;
            }
            else if (obj is byte b)
            {
                outRom.WriteByte(b);
                opos++;
            }
        }

        if (oldPos != null)
            outRom.Position = oldPos.Value;

        bix++;
    }
}

