using GaiaLib;
using GaiaLib.Asm;
using GaiaPacker;
using System.Globalization;
using System.Text.Json;

char[]
    _whitespace = [' ', '\t'],
    _operators = ['-', '+'],
    _commaspace = [',', ' ', '\t'],
    _addressspace = ['@', '&', '^'];

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

uint WriteFile(Process.ChunkFile file, IDictionary<string, Location> chunkLookup)
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
                ParseAssembly(inFile, chunkLookup);
                goto Next;

                //case "Assembly":
                //    ParseAssembly(inFile);
                //    continue;
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
    ///TODO: Add parsers for strings
    throw new($"Unable to get size for operand '{obj}'");
}

void ParseAssembly(Stream inStream, IDictionary<string, Location> chunkLookup)
{
    using var reader = new StreamReader(inStream);

    var blocks = new List<AsmBlock>();
    AsmBlock current, target;
    var tags = new Dictionary<string, string>();
    int ix, bix = 0, lineCount = 0;

    blocks.Add(current = new AsmBlock { Location = (uint)outRom.Position });

    while (!reader.EndOfStream)
    {
        var line = reader.ReadLine() ?? "";
        lineCount++;

        //Ignore comments
        if ((ix = line.IndexOf("--")) >= 0)
            line = line[..ix];

        line = line.Trim();
        if (line.Length == 0)
            continue;

        //Make everything case-sensitive
        line = line.ToUpper();

        //Process directives
        if (line[0] == '?')
        {
            continue;
        }

        //Process tags
        if (line[0] == '!')
        {
            line = line[1..];
            string name = line, value = null;
            if ((ix = line.IndexOfAny(_whitespace)) >= 0)
            {
                name = line[..ix];
                value = line[(ix + 1)..].Trim();
            }

            tags[name] = value;
            continue;
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


        //Process raw data
        if (line[0] == '#')
        {
            bool reverse = false;
            if (line[1] == '$')
            {
                reverse = true;
                line = line[2..];
            }
            else
                line = line[1..];

            var data = Convert.FromHexString(line);
            if (reverse)
                for (int x = 0, y = data.Length; --y > x;)
                {
                    var sample = data[x];
                    data[x++] = data[y];
                    data[y] = sample;
                }

            current.ObjList.Add(data);
            current.Size += (uint)data.Length;
            continue;
        }


        //Process labels
        if ((ix = line.IndexOf(':')) >= 0)
        {
            var label = line[..ix].Trim();

            blocks.Add(current = new AsmBlock { Location = current.Location + current.Size });
            bix++;

            if (uint.TryParse(label, NumberStyles.HexNumber, null, out var addr))
                current.Location = addr;
            else
                current.Label = label;

            continue;
        }

        string? mnem, operand = null;

        if ((ix = line.IndexOfAny(_whitespace)) > 0)
        {
            mnem = line[..ix];
            operand = line[(ix + 1)..].Trim();
        }
        else
            mnem = line;

        if (!OpCode.Grouped.TryGetValue(mnem, out var codes))
            throw new($"Unknown instruction line {lineCount}: '{line}'");

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

        object opnd = null;
        if (opCode == null)
        {
            if (mnem.StartsWith('J'))
            {
                opCode = codes.First(x => x.Mode == AddressingMode.Absolute || x.Mode == AddressingMode.AbsoluteLong);
                opnd = blocks.FirstOrDefault(x => x.Label?.Equals(operand) == true)
                //if (!blocks.Any(x => x.Label?.Equals(operand) == true))
                    ?? throw new($"Unable to locate target for label line {lineCount}: '{operand}'");
            }
            else
                throw new($"Unable to determine mode/code line {lineCount}: '{line}'");
        }

        current.ObjList.Add(new Op() { Code = opCode, Operands = [opnd ?? operand], Size = (byte)size });
        current.Size += (uint)size;
    }

    bix = 0;
    foreach (var block in blocks)
    {
        uint? oldPos = null;
        if (block.Label == null)
        {
            oldPos = (uint)outRom.Position;
            outRom.Position = block.Location;
        }
        else
            block.Location = (uint)outRom.Position;

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
                    if (label.StartsWith("#$"))
                    {
                        label = label[2..];
                        if (label.Length > 4)
                        {
                            var offset = int.Parse(label, NumberStyles.HexNumber);
                            offset -= (int)block.Location + opos;
                            if (op.Code.Mode == AddressingMode.PCRelativeLong)
                                op.Operands[0] = (ushort)offset;
                            else
                                op.Operands[0] = (byte)offset;
                        }
                        else
                            op.Operands[0] = label;
                    }
                    else
                    {
                        target = blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
                            ?? throw new($"Unable to find label {label}");

                        int offset = 0,
                            tix = blocks.IndexOf(target),
                            cic = oix;


                        if (tix <= bix)
                        {
                            while (tix < bix)
                            {
                                var b = blocks[tix++];
                                if (b.Label != null)
                                    offset += (int)b.Size;
                            }

                            while (--cic >= 0)
                                offset += getSize(objList[cic]);

                            offset = -offset;
                        }
                        else
                        {
                            while (--tix > bix)
                            {
                                var b = blocks[tix];
                                if (b.Label != null)
                                    offset += (int)b.Size;
                            }

                            while (cic < objList.Count)
                                offset += getSize(objList[cic++]);
                        }

                        outRom.WriteByte((byte)offset);
                        if (op.Size == 3)
                            outRom.WriteByte((byte)(offset >> 8));

                        continue;
                    }
                }

                foreach (var opnd in op.Operands)
                {
                    object result;

                    if (opnd is AsmBlock tgt)
                        result = op.Code.Mode == AddressingMode.PCRelativeLong
                            ? (tgt.Location + 0x800000u)
                            : (ushort)tgt.Location;
                    else if (opnd is string str)
                        result = (str.Length) switch
                        {
                            2 => result = byte.Parse(str, NumberStyles.HexNumber),
                            4 => result = ushort.Parse(str, NumberStyles.HexNumber),
                            6 => result = uint.Parse(str, NumberStyles.HexNumber),
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
        }

        if (oldPos != null)
            outRom.Position = oldPos.Value;

        bix++;
    }
}

