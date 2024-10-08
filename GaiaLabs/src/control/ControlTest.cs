using GaiaLabs;
using GaiaLib;
using GaiaLib.Database;
using GaiaLib.Rom;
using Godot;
using System;

public partial class ControlTest : Control
{
    //private ImageTexture _texture;



    public unsafe override void _EnterTree()
    {
        base._EnterTree();

        var baseDir = "C:\\Games\\GaiaLabs\\GaiaPacker\\bin\\Debug\\net8.0";
        var databasePath = baseDir + "\\database.json";
        var dbRoot = DbRoot.FromFile(databasePath);
        var state = RomState.FromScene(baseDir, dbRoot, "scene_meta", 1);

        //byte[] compData;
        //using(var file = File.OpenRead("C:\\Games\\Dump\\graphics\\bmp_002F3A.bin"))
        //{
        //    compData = new byte[file.Length];
        //    file.Read(compData);
        //}

        //var compressed = Compression.Compact(compData);

        ////using (var file = File.Create("C:\\Games\\Dump\\compressed.bin"))

        ////using (var file = File.OpenWrite("C:\\Games\\Illusion of Gaia.smc"))
        ////{
        ////    file.Position = 0x0F90E5;
        ////    file.Write(compressed);
        ////}

        //fixed (byte* ptr = compressed)
        //{
        //    var compare = Compression.Expand(ptr, compressed.Length);

        //    for (int i = 0; i < compare.Length; i++)
        //    {
        //        if (compare[i] != compData[i])
        //        {

        //        }
        //    }
        //}

        //using (var rom = File.OpenRead("C:\\Games\\SNES\\Illusion Of Gaia.smc"))
        //using (var outRom = File.Create("C:\\Games\\GaiaLabs.smc"))
        //{
        //    int sample;
        //    while ((sample = rom.ReadByte()) >= 0)
        //        outRom.WriteByte((byte)sample);

        //    while (outRom.Position < 0x400000)
        //        outRom.WriteByte(0);

        //    outRom.Position = 0xFFD7;
        //    outRom.WriteByte(0x0C);

        //    outRom.Position = 0x200002;
        //    using (var file = File.OpenRead("C:\\Games\\Dump\\graphics\\bmp_002F3A.bin"))
        //    {
        //        while ((sample = file.ReadByte()) >= 0)
        //            outRom.WriteByte((byte)sample);
        //    }
        //}

        ////return;
        //var ldr = RomLoader.Load("C:\\Games\\SNES\\Illusion Of Gaia.smc");

        ////Process.Repack("C:\\Games\\Dump");
        ////Process.ReadMetaXrefs(ldr._baseAddress);

        //try
        //{
        //    ldr.DumpDatabase("C:\\Games\\Dump");
        //}
        //catch (Exception ex)
        //{

        //}

        //var data = LZ77.Expand(ldr._basePtr + 0x176191, 0x800);
        //var data2 = LZ77.Expand(ldr._basePtr + 0x176852, 0x800);
        //var data3 = LZ77.Expand(ldr._basePtr + 0x176EFF, 0x800);

        //using (var f = new FileStream("C:\\Games\\SNES\\Tiledump1.bin", FileMode.Create, System.IO.FileAccess.Write))
        //    f.Write(data);

        //using (var f = new FileStream("C:\\Games\\SNES\\Tiledump2.bin", FileMode.Create, System.IO.FileAccess.Write))
        //    f.Write(data2);

        //using (var f = new FileStream("C:\\Games\\SNES\\Tiledump3.bin", FileMode.Create, System.IO.FileAccess.Write))
        //    f.Write(data3);

        //var data = LZ77.Expand(ldr._basePtr + 0x1753FD);

        //var data = LZ77.Expand(ldr._basePtr + 0x14F199, 0x2000, 0xDA6);
        //var data2 = LZ77.Expand(ldr._basePtr + 0x14FF3F, 0x2000, 0xC1);
        //var data3 = LZ77.Expand(ldr._basePtr + 0x150000, 0x2000, 0x8E6);

        //using (var f = new FileStream("C:\\Games\\SNES\\Mapdump1.bin", FileMode.Create, System.IO.FileAccess.Write))
        //    f.Write(data);

        //using (var f = new FileStream("C:\\Games\\SNES\\Mapdump2.bin", FileMode.Create, System.IO.FileAccess.Write))
        //    f.Write(data2);

        //using (var f = new FileStream("C:\\Games\\SNES\\Mapdump3.bin", FileMode.Create, System.IO.FileAccess.Write))
        //    f.Write(data3);
    }

    public override void _Ready()
    {
        base._Ready();

        //using (var loader = RomLoader.Load("C:\\Games\\SNES\\Illusion Of Gaia.smc"))
        //{
        //    //var image = ImageConverter.ReadImage(loader._basePtr, 128, 128, 4, 0);
        //    Address addr = 0xC02F3C;
        //    Location loc = 0x10002;

        //    var data = LZ77.Expand(loader._basePtr + 0x1e1635, 0x1af7, 0x3000);
        //    var image = ImageConverter.ReadImage(data, 128, 128, 4, 0);

        //    _texture = ImageTexture.CreateFromImage(image);
        //}
    }


    public override void _Draw()
    {
        //DrawTextureRect(_texture, new(0, 0, 1024, 1024), false);
    }
}
