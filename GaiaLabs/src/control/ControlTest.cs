using GaiaLabs;
using GaiaLib;
using GaiaLib.Database;
using GaiaLib.Rom;
using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class ControlTest : Control
{
    //private ImageTexture _texture;
    internal static byte[] PaletteData;
    internal static RomState RomState;
    internal static byte[] TilesetBitmap;
    internal static Image TilesetImage;
    internal static ImageTexture TilesetTexture;
    internal static byte[] TilemapBitmap;
    internal static Image TilemapImage;
    internal static ImageTexture TilemapTexture;
    internal static float TilemapRatio;
    internal static int TilemapTileWidth, TilemapTileHeight;
    internal static int SelectedIndex;
    internal static int CurrentScene;

    internal static ProjectRoot Project;
    internal static DbRoot DbRoot;
    internal static bool IsEffect;

    internal static int _mapWidth, _mapHeight;


    public unsafe override void _EnterTree()
    {
        Project = ProjectRoot.Load();
        DbRoot = DbRoot.FromFile(Project.DatabasePath);
        LoadScene(0x01);

        base._EnterTree();
    }

    public static void LoadScene(int id, bool effect = false)
    {
        var state = RomState = RomState.FromScene(Project.BaseDir, DbRoot, "scene_meta", id);



        var set = effect ? state.EffectTileset : state.MainTileset;
        var pal = state.CGRAM;
        var vram = state.VRAM;

        var fullPalette = PaletteData = new byte[256 * 4];
        var fullTexture = TilesetBitmap = new byte[16 * 16 * 16 * 16 * 4];

        byte convert(int color) => (byte)(int)(color * ImageConverter._sample5to8);

        for (int i = 0, x = 0; i < pal.Length;)
        {
            int sample = pal[i++] | (pal[i++] << 8);
            fullPalette[x++] = convert(sample & 0x1F);
            fullPalette[x++] = convert((sample >> 5) & 0x1F);
            fullPalette[x++] = convert((sample >> 10) & 0x1F);
            fullPalette[x++] = 255;
        }

        var tileIx = 0;
        byte[] indexBuffer = new byte[8];
        int[] offsetBuffer = new int[2];

        for (int ty = 0; ty < 32; ty++)
            for (int tx = 0; tx < 8 && tileIx < set.Length; tx++)
            {
                for (int quad = 0; quad < 4; quad++)
                {
                    var tileData = set[tileIx++] | (set[tileIx++] << 8);
                    var tile = tileData & 0x01FF;
                    var flag = tileData & 0x0200;
                    var priority = (tileData & 0x2000) != 0;
                    var hMirror = (tileData & 0x4000) != 0;
                    var vMirror = (tileData & 0x8000) != 0;
                    var vOffset = (tile << 5) + 0x4000;// + (effect ? 0x2000 : 0);
                    var pOffset = (tileData & 0x1C00) >> 4;

                    offsetBuffer[0] = vOffset;
                    offsetBuffer[1] = vOffset + 16;

                    int row = vMirror ? 8 : -1;
                    Func<bool> checkRow = vMirror ? (() => row-- > 0) : (() => ++row < 8);
                    while (checkRow())
                    {
                        //Clear index buffer
                        Array.Clear(indexBuffer);

                        //Rotate bits from samples
                        for (byte plane = 0, planeBit = 1; plane < 4; plane++, planeBit <<= 1)
                            for (byte i = 0, testBit = 0x80, sample = vram[offsetBuffer[plane >> 1]++]; i < 8; i++, testBit >>= 1)
                                if ((sample & testBit) != 0)
                                    indexBuffer[i] |= planeBit;

                        //Output data
                        //var swap = height >= 256 && (ty & 1) == 1;
                        var cOffset = (((ty << 4) + row + (quad > 1 ? 8 : 0)) << 7) + ((tx << 4) + ((quad & 1) == 1 ? 8 : 0));

                        cOffset <<= 2;

                        int col = hMirror ? 8 : -1;
                        Func<bool> checkCol = hMirror ? (() => col-- > 0) : (() => ++col < 8);
                        byte cIndex;
                        while (checkCol())
                            //for (byte i = 0, sample; i < 8; i++)
                            if ((cIndex = indexBuffer[col]) == 0) //Transparent pixel
                            {
                                fullTexture[cOffset++] = 0;
                                fullTexture[cOffset++] = 0;
                                fullTexture[cOffset++] = 0;
                                fullTexture[cOffset++] = 0;
                            }
                            else
                            {
                                var zOffset = pOffset + (cIndex << 2);
                                fullTexture[cOffset++] = fullPalette[zOffset++];
                                fullTexture[cOffset++] = fullPalette[zOffset++];
                                fullTexture[cOffset++] = fullPalette[zOffset++];
                                fullTexture[cOffset++] = fullPalette[zOffset];
                            }
                    }
                }
            }

        TilesetImage = Image.CreateFromData(128, 512, false, Image.Format.Rgba8, fullTexture);
        TilesetTexture = ImageTexture.CreateFromImage(TilesetImage);

        var map = effect ? state.EffectTilemap : state.MainTilemap;
        int tileWidth = TilemapTileWidth = effect ? state.EffectTilemapW : state.MainTilemapW;
        int tileHeight = TilemapTileHeight = effect ? state.EffectTilemapH : state.MainTilemapH;
        _mapWidth = tileWidth << 8;
        _mapHeight = tileHeight << 8;
        int mapOffset = 0;

        var mapTextureBytes = TilemapBitmap = new byte[(_mapWidth * _mapHeight) << 2];

        for (int py = 0; py < tileHeight; py++)
            for (int px = 0; px < tileWidth; px++)
            {
                for (int ty = 0; ty < 16; ty++)
                    for (int tx = 0; tx < 16 && mapOffset < map.Length; tx++)
                    {
                        var index = map[mapOffset++];
                        var srcOffset = ((index & 0xF8) << 8) | ((index & 0x07) << 4);
                        var dstOffset = (((py << 4) | ty) << 4) * _mapWidth + (((px << 4) | tx) << 4);
                        srcOffset <<= 2;
                        dstOffset <<= 2;

                        for (int y = 0; y < 16; y++)
                        {
                            for (int x = 0; x < 16 * 4; x++)
                            {
                                mapTextureBytes[dstOffset + x] = fullTexture[srcOffset + x];
                            }
                            srcOffset += 16 * 8 * 4;
                            dstOffset += _mapWidth << 2;
                        }
                    }
            }

        TilemapImage = Image.CreateFromData(_mapWidth, _mapHeight, false, Image.Format.Rgba8, mapTextureBytes);
        TilemapTexture = ImageTexture.CreateFromImage(TilemapImage);
        TilemapRatio = (float)_mapWidth / _mapHeight;
        IsEffect = effect;
        CurrentScene = id;

        TilemapControl.Instance?.Reset();
        TilesetControl.Instance?.QueueRedraw();
    }

    public override void _Ready()
    {
        base._Ready();
    }

    //private Vector2 GetDrawSize()
    //{
    //    var ratio = Size.X / Size.Y;
    //    return ratio > TilemapRatio
    //        ? new(Size.Y * TilemapRatio, Size.Y)
    //        : new(Size.X, Size.X / TilemapRatio);
    //}

    //public override void _Draw()
    //{
    //    _drawSize = GetDrawSize();
    //    _tileSize = _drawSize.X / (RomState.MainTilemapW << 4);
    //    DrawTextureRect(TilemapTexture, new(0, 0, _drawSize), false);
    //    DrawPolyline(_hoverBox, Color.Color8(0, 255, 255));
    //}

    //public override void _Input(InputEvent @event)
    //{
    //    base._Input(@event);

    //    if (@event is InputEventMouse motion)
    //    {
    //        var pos = motion.Position;
    //        if (pos.X < 0 || pos.Y < 0 || pos.X > _drawSize.X || pos.Y > _drawSize.Y)
    //        {
    //            _hoverBox[0] = _hoverBox[1] = _hoverBox[2] = _hoverBox[3] = _hoverBox[4] = new(-1, -1);
    //            _hoverTileX = -1;
    //            _hoverTileY = -1;
    //        }
    //        else
    //        {
    //            _hoverTileX = (int)(pos.X / _tileSize);
    //            _hoverTileY = (int)(pos.Y / _tileSize);

    //            var posX = _hoverTileX * _tileSize;
    //            var posY = _hoverTileY * _tileSize;

    //            _hoverBox[0] = _hoverBox[4] = new(posX, posY);
    //            _hoverBox[1] = new(posX + _tileSize, posY);
    //            _hoverBox[2] = new(posX + _tileSize, posY + _tileSize);
    //            _hoverBox[3] = new(posX, posY + _tileSize);
    //        }

    //        QueueRedraw();
    //    }

    //    if (@event is InputEventMouseButton mouse)
    //    {
    //        if (mouse.Pressed && mouse.ButtonIndex == MouseButton.Right)
    //        {
    //            if (_hoverTileX >= 0 && _hoverTileY >= 0)
    //            {
    //                var mOffset = (_hoverTileY >> 4) * RomState.MainTilemapW + (_hoverTileX >> 4);

    //                SelectedIndex = RomState.MainTilemap[(mOffset << 8) | ((_hoverTileY & 0x0F) << 4) | (_hoverTileX & 0x0F)];
    //            }
    //        }
    //        else if (mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
    //        {
    //            if (_hoverTileX >= 0 && _hoverTileY >= 0)
    //            {
    //                //var srcOffset = ((SelectedIndex & 0xF0) << 8) | ((SelectedIndex & 0x0F) << 4);
    //                //var dstOffset = (_hoverTileY << 4) * _mapWidth + (_hoverTileX << 4);
    //                //srcOffset <<= 2;
    //                //dstOffset <<= 2;

    //                //for (int y = 0; y < 16; y++)
    //                //{
    //                //    for (int x = 0; x < 16 * 4; x++)
    //                //        TilemapBitmap[dstOffset + x] = TilesetBitmap[srcOffset + x];

    //                //    srcOffset += 16 * 16 * 4;
    //                //    dstOffset += _mapWidth << 2;
    //                //}
    //                var mOffset = (_hoverTileY >> 4) * RomState.MainTilemapW + (_hoverTileX >> 4);

    //                RomState.MainTilemap[(mOffset << 8) | ((_hoverTileY & 0x0F) << 4) | (_hoverTileX & 0x0F)] = (byte)SelectedIndex;

    //                //TilemapImage.Dispose();
    //                //TilemapImage = Image.CreateFromData(_mapWidth, _mapHeight, false, Image.Format.Rgb8, )
    //                TilemapImage.BlitRect(TilesetImage,
    //                    new((SelectedIndex & 0x0F) << 4, SelectedIndex & 0xF0, 16, 16),
    //                    new(_hoverTileX << 4, _hoverTileY << 4));

    //                //TilemapImage.SetData(_mapWidth, _mapHeight, false, Image.Format.Rgba8, TilemapBitmap);

    //                TilemapTexture.Update(TilemapImage);

    //                //TilemapImage = Image.from

    //                QueueRedraw();
    //            }
    //        }
    //    }
    //}

    public static void SaveMap()
    {
        var map = RomState.MainTilemap;
        var outSize = (RomState.MainTilemapW * RomState.MainTilemapH) << 8;

        using var file = File.Create(RomState.MainTilemapPath);

        file.WriteByte(RomState.MainTilemapW);
        file.WriteByte(RomState.MainTilemapH);
        for (int x = 0; x < outSize; x++)
            file.WriteByte(map[x]);
    }
}
