using GaiaLabs;
using Godot;
using System;
using System.IO;

public partial class ControlTest : Control
{
    //private ImageTexture _texture;

    public override void _EnterTree()
    {
        base._EnterTree();

        var ldr = RomLoader.Load("C:\\Games\\SNES\\Illusion Of Gaia.smc");

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
