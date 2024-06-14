using GaiaPacker;
using System.Globalization;
using System.IO.Enumeration;
using System.Text.Json;

uint TilesetLoadEntry = 0x02876Du;
byte[] TilesetLoadPatch = [
    0xA7, 0x3E, 0x85, 0x78, 0xE6, 0x3E, 0xE6, 0x3E,
    0xC9, 0x00, 0x00, 0xF0, 0x04, 0x5C, 0x77, 0x87,
    0x82, 0x5C, 0x8A, 0x87, 0x82
];

uint TilemapLoadEntry = 0x028914u;
byte[] TilemapLoadPatch = [
    0xA5, 0x01, 0x9D, 0x93, 0x06, 0xEB, 0xA5, 0x03,
    0x9D, 0x97, 0x06, 0xAD, 0x67, 0x06, 0x9D, 0x9B,
    0x06, 0xC2, 0x20, 0x5C, 0x20, 0x89, 0x82
];

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

//Apply tileset loading patch
outRom.Position = 0x00F48F;
ApplyPatch(TilesetLoadEntry, TilesetLoadPatch);
ApplyPatch(TilemapLoadEntry, TilemapLoadPatch);

GaiaLib.Process.Repack("C:\\Games\\Dump", "C:\\Games\\GaiaLabs\\GaiaLabs\\database.json", file =>
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

                //case "Assembly":
                //    ParseAssembly(inFile);
                //    continue;
        }

        //Mark as not compressed
        if (file.File.Compressed)
        {
            outRom.WriteByte(0);
            outRom.WriteByte(0);
        }

        //Copy file to rom
        int sample;
        while ((sample = inFile.ReadByte()) >= 0)
            outRom.WriteByte((byte)sample);
    }

    //Write reference fixups
    //var nextPos = outRom.Position;
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
});

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

void ApplyPatch(uint entry, byte[] asm)
{
    var pos = (uint)outRom.Position;

    outRom.Position = entry;
    outRom.WriteByte(0x5C);
    outRom.WriteByte((byte)pos);
    outRom.WriteByte((byte)(pos >> 8));
    outRom.WriteByte((byte)((pos >> 16) + 0xC0));

    outRom.Position = pos;
    outRom.Write(asm);
}

void ParseAssembly(Stream inStream)
{
    using var reader = new StreamReader(inStream);

    var blocks = new List<AsmBlock>();
    AsmBlock currentBlock = new AsmBlock();

    while (!reader.EndOfStream)
    {
        var line = reader.ReadLine() ?? "";
        var labelIx = line.IndexOf(':');
        if (labelIx >= 0)
        {
            var label = line[..labelIx].Trim().ToUpper();

            blocks.Add(currentBlock = new AsmBlock { });

            if (uint.TryParse(label, NumberStyles.HexNumber, null, out var addr))
                currentBlock.Location = addr;
            else
                currentBlock.Label = label;

            break;
        }

        line = line.Trim().ToUpper();
        string? mnem, operand = null;

        var ix = line.IndexOf(' ');
        if (ix > 0)
        {
            mnem = line[..ix];
            operand = line[(ix + 1)..].Trim();
        }
        else
            mnem = line;

        if (Asm.OpCodes.TryGetValue(mnem, out var codes))
        {
            var opCode = codes.First();

            if (operand != null)
                foreach (var code in codes)
                {
                    if (Asm.AddressingRegex.TryGetValue(code.Mode, out var regex))
                    {
                        var match = regex.Match(operand);
                        if (match.Success)
                        {
                            operand = match.Captures[0].Value;
                            opCode = code;
                            break;
                        }
                    }
                }

            currentBlock.OpList.Add(new() { Code = opCode, Operand = operand });
        }
    }
}

