using GaiaLib;
using GaiaPacker;
using System.Globalization;
using System.Text.Json;

uint TilesetLoadEntry = 0x02876D;
byte[] TilesetLoadPatch = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0xF0, 0x07,         //  BEQ #$07
    0x30, 0x05,         //  BMI #$05
    0x85, 0x78,         //  STA $78
    0x4C, 0x77, 0x87,   //  JMP $8777
    0x4C, 0x8A, 0x87,   //  JMP $878A
];

uint TilemapLoadEntry1 = 0x02883F;
byte[] TilemapLoadPatch1 = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0xF0, 0x0A,         //  BEQ #$0A
    0x30, 0x08,         //  BMI #$08
    0x85, 0x78,         //  STA $78
    0x9D, 0x66, 0x06,   //  STA $0666
    0x4C, 0x4F, 0x88,   //  JMP $884F
    0x4C, 0xB5, 0x88,   //  JMP $88B5
];

uint TilemapLoadEntry2 = 0x028928F;
byte[] TilemapLoadPatch2 = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0xF0, 0x07,         //  BEQ #$07
    0x30, 0x05,         //  BMI #$05
    0x85, 0x78,         //  STA $78
    0x4C, 0x43, 0x89,   //  JMP $8943
    0x4C, 0x35, 0x89,   //  JMP $8935
];

uint TilemapLoadEntry3 = 0x028914;
byte[] TilemapLoadPatch3 = [
    0xA5, 0x01,         //  LDA $01
    0x9D, 0x93, 0x06,   //  STA $0693, X    Copy stored width
    //0xEB,               //  XBA
    0xA5, 0x03,         //  LDA $03
    0x9D, 0x97, 0x06,   //  STA $0697, X    Copy stored height
    0xAD, 0x67, 0x06,   //  LDA $0667
    0x9D, 0x9B, 0x06,   //  STA $069B, X    Copy stored multiply result (used by 0 index)
    0xC2, 0x20,         //  REP #$20
    //0x9C, 0x64, 0x06,   //  STZ $0664       Zero src offset
    0x9C, 0x68, 0x06,   //  STZ $0668       Zero dst offset
    0x4C, 0x20, 0x89,   //  JMP $8920
];

uint Meta17LoadEntry = 0x028C61;
byte[] Meta17LoadPatch = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0xF0, 0x05,         //  BEQ #$05
    0x30, 0x03,         //  BMI #$03
    0x4C, 0x69, 0x8C,   //  JMP $8C69
    0x4C, 0x85, 0x8C,   //  JMP $8C85
];

uint SpritemapLoadEntry = 0x028C01;
byte[] SpritemapLoadPatch = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0xF0, 0x05,         //  BEQ #$05
    0x30, 0x03,         //  BMI #$03
    0x4C, 0x0C, 0x8C,   //  JMP $8C0C
    0x4C, 0x1B, 0x8C,   //  JMP $8C1B
];

uint BitmapLoadEntry1 = 0x028503;
byte[] BitmapLoadPatch1 = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0xF0, 0x07,         //  BEQ Mode7Check
    0x30, 0x05,         //  BMI Mode7Check
    0x85, 0x78,         //  STA $78
    0x4C, 0x10, 0x85,   //  JMP $8510

    //Mode7Check:
    0xE0, 0x6C, 0x06,   //  CPX #$066C
    0xD0, 0x0A,         //  BNE JumpDma
    0xAD, 0xEE, 0x06,   //  LDA $06EE
    0x89, 0x00, 0x08,   //  BIT #$0800
    0xF0, 0x02,         //  BEQ JumpDma
    0x80, 0x03,         //  BRA Mode7Process
    
    //JumpDma:
    0x4C, 0x60, 0x85,   //  JMP $8560

    //Mode7Process:
    0x5A,               //  PHY
    0x8B,               //  PHB
  
    0xA5, 0x3E,         //  LDA $3E
    0x85, 0x72,         //  STA $72
    0x18,               //  CLC
    0x69, 0x10, 0x00,   //  ADC #$0010
    0x85, 0x75,         //  STA $75
  
    0xE2, 0x20,         //  SEP #$20
    0xA5, 0x40,         //  LDA $40
    0x85, 0x74,         //  STA $74
    0x85, 0x77,         //  STA $77
  
    0xA9, 0x7E,         //  LDA #$7E
    0x48,               //  PHA
    0xAB,               //  PLB
  
    0x64, 0x0E,         //  STZ $0E
    0xA2, 0x00, 0xA0,   //  LDX #$A000
    0x86, 0x5E,         //  STX $5E
    0x20, 0x8C, 0x86,   //  JSR $868C
    0xA0, 0x00, 0x00,   //  LDY #$0000
  
    //  ReadLookup:
    0xA9, 0x07,         //  LDA #$07
    0x85, 0x12,         //  STA $12
    0xB2, 0x3E,         //  LDA ($3E)
    0x85, 0x10,         //  STA $10
    0xE6, 0x3E,         //  INC $3E
    0xD0, 0x02,         //  BNE #$02
    0xE6, 0x3F,         //  INC $3F
  
    //  ReadSample:
    0xB7, 0x72,         //  LDA [$72],Y
    0x85, 0x00,         //  STA $00
    0xB7, 0x75,         //  LDA [$75],Y
    0x85, 0x04,         //  STA $04
    0xC8,               //  INY
    0xB7, 0x72,         //  LDA [$72],Y
    0x85, 0x02,         //  STA $02
    0xB7, 0x75,         //  LDA [$75],Y
    0x85, 0x06,         //  STA $06
    0xA2, 0x07, 0x00,   //  LDX #$0007
  
    //  RotateBits:
    0xA9, 0x00,         //  LDA #$00
    0x26, 0x06,         //  ROL $06
    0x2A,               //  ROL
    0x26, 0x04,         //  ROL $04
    0x2A,               //  ROL
    0x26, 0x02,         //  ROL $02
    0x2A,               //  ROL
    0x26, 0x00,         //  ROL $00
    0x2A,               //  ROL
    0x05, 0x10,         //  ORA $10
    0x92, 0x5E,         //  STA ($5E)
    0xE6, 0x5E,         //  INC $5E
    0xD0, 0x02,         //  BNE #$02
    0xE6, 0x5F,         //  INC $5F
  
    0xCA,               //  DEX
    0x10, 0xE5,         //  BPL RotateBits
    0xC8,               //  INY
    0xC6, 0x12,         //  DEC $12
    0x10, 0xCC,         //  BPL ReadSample
    0xC2, 0x20,         //  REP #$20
    0x98,               //  TYA
    0x18,               //  CLC
    0x69, 0x10, 0x00,   //  ADC #$0010
    0xA8,               //  TAY
    0xE2, 0x20,         //  SEP #$20
    0xC0, 0x00, 0x20,   //  CPY #$2000
    0x90, 0xAF,         //  BCC ReadLookup
    
    0x9C, 0x71, 0x06,   //  STZ $671   Clear "last bank" so next resource loads
    0x9C, 0x7D, 0x06,   //  STZ $67D
    0x9C, 0x80, 0x06,   //  STZ $680
    0x9C, 0x83, 0x06,   //  STZ $683

    0x4C, 0x5D, 0x86,   //  JMP $865D
];

uint BitmapLoadEntry2 = 0x028592;
byte[] BitmapLoadPatch2 = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0xF0, 0x07,         //  BEQ #$07
    0x30, 0x05,         //  BMI #$05
    0x85, 0x78,         //  STA $78
    0x4C, 0x9F, 0x85,   //  JMP $859F
    0x4C, 0xB2, 0x85,   //  JMP $85B2
];

//Can't use DMA without screen blank, use MVN instead
uint Cop51LoadEntry = 0x0099A8;
byte[] Cop51LoadPatch = [
    0xA7, 0x3E,         //  LDA [$3E]
    0xC9, 0x00, 0x00,   //  CMP #$0000
    0x30, 0x10,         //  BMI HandleMinus
    0xF0, 0x09,         //  BEQ HandleZero
    0x85, 0x78,         //  STA $78
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0x4C, 0xB0, 0x99,   //  JMP $99B0

    //HandleZero:
    0xA9, 0x00, 0x20,   //  LDA  #$2000
    0x80, 0x06,         //  BRA DoMVN

    //HandleMinus:
    0xA9, 0x00, 0x00,   //  LDA #$0000
    0x38,               //  SEC
    0xE7, 0x3E,         //  SBC [$3E]

    //DoMVN:
    0xE6, 0x3E,         //  INC $3E
    0xE6, 0x3E,         //  INC $3E
    0xDA,               //  PHX
    0x5A,               //  PHY

    0x38,               //  SEC
    0xE9, 0x01, 0x00,   //  SBC #$0001
    0x48,               //  PHA
    
    0xE2, 0x20,         //  SEP #$20
    0xA9, 0x7E,         //  LDA #$7E
    0x8D, 0x04, 0x04,   //  STA $0404
    0xA5, 0x40,         //  LDA $40
    0x8D, 0x05, 0x04,   //  STA $0405
    0xC2, 0x20,         //  REP #$20
    
    0xA6, 0x3E,         //  LDX $3E
    0xA4, 0x7A,         //  LDY $7A

    0x68,               //  PLA
    0x20, 0x02, 0x04,   //  JSR $0402
    0x7A,               //  PLY
    0xFA,               //  PLX

    0x4C, 0xB8, 0x99,   //  JMP $99B8
];


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
outRom.Position = 0x00F48F;
ApplyPatch(Cop51LoadEntry, Cop51LoadPatch);

//Apply meta patches
outRom.Position = 0x02F08C;
ApplyPatch(TilesetLoadEntry, TilesetLoadPatch);
ApplyPatch(TilemapLoadEntry1, TilemapLoadPatch1);
ApplyPatch(TilemapLoadEntry2, TilemapLoadPatch2);
ApplyPatch(TilemapLoadEntry3, TilemapLoadPatch3);
ApplyPatch(SpritemapLoadEntry, SpritemapLoadPatch);
ApplyPatch(Meta17LoadEntry, Meta17LoadPatch);
ApplyPatch(BitmapLoadEntry1, BitmapLoadPatch1);
ApplyPatch(BitmapLoadEntry2, BitmapLoadPatch2);


//Debugman
//foreach (var loc in DebugmanEntries)
//    ApplyData(loc, DebugmanActor);

Process.Repack("C:\\Games\\Dump", "C:\\Games\\GaiaLabs\\GaiaLabs\\database.json", WriteFile, WriteSfx);

uint WriteSfx(Location location, IEnumerable<string> paths)
{
    var offset = location.Offset;
    outRom.Position = offset;

    void writeByte(byte b)
    {
        outRom.WriteByte(b);
        if ((++offset & 0x8000u) != 0)
        {
            offset = (offset & 0x3F0000) + 0x10000;// | (offset & 0x7FFFu);
            outRom.Position = offset;
        }
    }

    foreach (var path in paths)
    {
        using var inFile = File.OpenRead(path);

        writeByte((byte)inFile.Length);
        writeByte((byte)(inFile.Length >> 8));

        int sample;
        while ((sample = inFile.ReadByte()) >= 0)
            writeByte((byte)sample);
    }

    return (uint)outRom.Position;
}

void WriteFile(Process.ChunkFile file)
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

