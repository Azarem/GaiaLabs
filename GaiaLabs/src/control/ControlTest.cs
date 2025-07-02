using GaiaLabs;
using GaiaLib;
using GaiaLib.Database;
using GaiaLib.Rom;
using GaiaLib.Sprites;
using Godot;
using System;
using System.IO;
using System.Text.Json;
using static Godot.WebSocketPeer;

public partial class ControlTest : Control
{
	public const float _sample4to5 = 31.3f / 15f;
	public const float _sample4to6 = 63.3f / 15f;
	public const float _sample4to8 = 255.3f / 15f;
	public const float _sample5to8 = 255.3f / 31f;
	public static ControlTest Instance { get; private set; }

	//private ImageTexture _texture;
	internal static byte[] PaletteData;
	internal static RomState RomState;
	internal static byte[] TilemapCurrent { get => IsEffect ? RomState.EffectTilemap : RomState.MainTilemap; }
	internal static byte[] TilesetCurrent { get => IsEffect ? RomState.EffectTileset : RomState.MainTileset; }
	internal static byte[] TilesetBitmap;
	internal static Image TilesetImage;
	internal static ImageTexture TilesetTexture;
	internal static byte[] TilemapBitmap;
	internal static Image TilemapImage;
	internal static ImageTexture TilemapTexture;
	internal static float TilemapRatio;
	internal static int TilemapWidth { get => IsEffect ? RomState.EffectTilemapW : RomState.MainTilemapW; }
	internal static int TilemapHeight { get => IsEffect ? RomState.EffectTilemapH : RomState.MainTilemapH; }
	internal static int SelectedIndex;
	internal static int CurrentScene;

	internal static ProjectRoot Project;
	internal static DbRoot DbRoot;
	internal static bool IsEffect;
	internal static bool UseOffset;

	internal static byte[] GfxBitmap = new byte[0x4000];
	internal static Image GfxImage;
	internal static ImageTexture GfxTexture;

	internal static SpriteMap SpriteMap { get => RomState.SpriteMap; }

	internal static int _mapWidth, _mapHeight;

	public override void _EnterTree()
	{
		Instance = this;
		Project = ProjectRoot.Load();
		DbRoot = DbRoot.FromFolder(Project.DatabasePath, Project.SystemPath);

		base._EnterTree();
	}

	public static void LoadScene(int id)
	{
		RomState = RomState.FromScene(Project.BaseDir, DbRoot, "scene_meta", id);
		CurrentScene = id;
		ReloadGraphicSet();
		ReloadTileset();
		ReloadTilemap();
		//SpriteTree.Instance?.Reset();
		SpriteSetList.Instance?.Reset();
		SpriteFrameList.Instance?.Reset();
		SpriteGroupList.Instance?.Reset();
	}

	public static void ReloadGraphicSet(int? palette = null)
	{
		//var set = TilesetCurrent;// IsEffect ? state.EffectTileset : state.MainTileset;
		//var pal = RomState.CGRAM;
		var vram = RomState.VRAM;

		//var fullPalette = PaletteData = new byte[256 * 4];
		var fullTexture = GfxBitmap = new byte[8 * 8 * 16 * 16 * 2 * 4];

		var pOffset = palette != null ? (palette.Value << 6) : 0;

		//byte convert(int color) => (byte)(int)(color * ImageConverter._sample5to8);

		//for (int i = 0, x = 0; i < pal.Length;)
		//{
		//    int sample = pal[i++] | (pal[i++] << 8);
		//    fullPalette[x++] = convert(sample & 0x1F);
		//    fullPalette[x++] = convert((sample >> 5) & 0x1F);
		//    fullPalette[x++] = convert((sample >> 10) & 0x1F);
		//    fullPalette[x++] = 255;
		//}

		//var tileIx = 0;
		byte[] indexBuffer = new byte[8];
		int[] offsetBuffer = new int[2];

		var vOffset = 0x4000 + (UseOffset ? 0x2000 : 0);

		for (int ty = 0; ty < 32; ty++)
			for (int tx = 0; tx < 16; tx++)
			{
				//for (int quad = 0; quad < 4; quad++)
				//{
				offsetBuffer[0] = vOffset;
				offsetBuffer[1] = vOffset + 16;
				vOffset += 32;

				for (int row = 0; row < 8; row++)
				{
					//Clear index buffer
					Array.Clear(indexBuffer);

					//Rotate bits from samples
					for (byte plane = 0, planeBit = 1; plane < 4; plane++, planeBit <<= 1)
						for (byte i = 0, testBit = 0x80, sample = vram[offsetBuffer[plane >> 1]++]; i < 8; i++, testBit >>= 1)
							if ((sample & testBit) != 0)
								indexBuffer[i] |= planeBit;

					var cOffset = (((ty << 3) + row) << (4 + 3)) + (tx << 3);

					cOffset <<= 2;

					byte cIndex;
					for (int col = 0; col < 8; col++)
						//for (byte i = 0, sample; i < 8; i++)
						if ((cIndex = indexBuffer[col]) == 0) //Transparent pixel
						{
							fullTexture[cOffset++] = 0;
							fullTexture[cOffset++] = 0;
							fullTexture[cOffset++] = 0;
							fullTexture[cOffset++] = 0;
						}
						else if (palette != null)
						{
							var zOffset = pOffset + (cIndex << 2);
							fullTexture[cOffset++] = PaletteData[zOffset++];
							fullTexture[cOffset++] = PaletteData[zOffset++];
							fullTexture[cOffset++] = PaletteData[zOffset++];
							fullTexture[cOffset++] = 0xFF;
						}
						else
						{
							var zOffset = (byte)(cIndex << 4);// pOffset + (cIndex << 2);
							fullTexture[cOffset++] = zOffset;
							fullTexture[cOffset++] = zOffset;
							fullTexture[cOffset++] = zOffset;
							fullTexture[cOffset++] = 0xFF;
						}
				}
				//}
			}

		GfxImage = Image.CreateFromData(128, 256, false, Image.Format.Rgba8, fullTexture);
		GfxTexture = ImageTexture.CreateFromImage(GfxImage);

		GfxSelector.Instance?.QueueRedraw();
	}

	public static void ReloadTileset()
	{
		var set = TilesetCurrent;// IsEffect ? state.EffectTileset : state.MainTileset;
		var pal = RomState.CGRAM;
		var vram = RomState.VRAM;

		var fullPalette = PaletteData = new byte[256 * 4];
		var fullTexture = TilesetBitmap = new byte[16 * 16 * 16 * 16 * 4];

		byte convert(int color) => (byte)(int)(color * _sample5to8);

		for (int i = 0, x = 0; i < pal.Length;)
		{
			int sample = pal[i++] | (pal[i++] << 8);
			fullPalette[x++] = convert(sample & 0x1F);
			fullPalette[x++] = convert((sample >> 5) & 0x1F);
			fullPalette[x++] = convert((sample >> 10) & 0x1F);
			fullPalette[x++] = 0xFF;
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
					var vOffset = (tile << 5) + 0x4000 + (UseOffset ? 0x2000 : 0);
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

		TilesetControl.Instance?.Reset();
		TilesetEditor.Instance?.Reset();
	}

	public static void ReloadTilemap()
	{
		var map = TilemapCurrent;// = IsEffect ? state.EffectTilemap : state.MainTilemap;
		int tileWidth = TilemapWidth;// = IsEffect ? state.EffectTilemapW : state.MainTilemapW;
		int tileHeight = TilemapHeight;// = IsEffect ? state.EffectTilemapH : state.MainTilemapH;
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
								mapTextureBytes[dstOffset + x] = TilesetBitmap[srcOffset + x];
							}
							srcOffset += 16 * 8 * 4;
							dstOffset += _mapWidth << 2;
						}
					}
			}

		TilemapImage = Image.CreateFromData(_mapWidth, _mapHeight, false, Image.Format.Rgba8, mapTextureBytes);
		TilemapTexture = ImageTexture.CreateFromImage(TilemapImage);
		TilemapRatio = (float)_mapWidth / _mapHeight;

		TilemapControl.Instance?.Reset();
		WidthEdit.Instance?.Reset();
		HeightEdit.Instance?.Reset();
	}

	public override void _Ready()
	{
		base._Ready();
		LoadScene(0x01);
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
		var map = TilemapCurrent;
		var outSize = (TilemapWidth * TilemapHeight) << 8;
		using var file = File.Create(IsEffect ? RomState.EffectTilemapPath : RomState.MainTilemapPath);

		file.WriteByte((byte)TilemapWidth);
		file.WriteByte((byte)TilemapHeight);
		for (int x = 0; x < outSize; x++)
			file.WriteByte(map[x]);
	}

	public static void SaveSet()
	{
		var set = TilesetCurrent;
		using var file = File.Create(IsEffect ? RomState.EffectTilesetPath : RomState.MainTilesetPath);

		for (int x = 0; x < 0x800; x++)
			file.WriteByte(set[x]);
	}

	public static void SaveSprites()
	{
		var spm = SpriteMap;
		if (spm == null)
			return;

		using var file = File.Create(RomState.SpriteMapPath);
		spm.ToStream(file);
	}

	public static void ChangeHeight(int height)
	{
		var isReverse = height < 0;
		if (isReverse)
			height = -height;

		var oldHeight = TilemapHeight;
		if (oldHeight == height)
			return;

		if (isReverse)
		{
			var newMap = new byte[0x2000];
			var oldMap = TilemapCurrent;
			var srcOffset = 0;
			var dstOffset = 0;
			var srcPos = 0;
			var dstPos = 0;
			var copyHeight = height;

			if (height > oldHeight)
			{
				dstOffset = height - oldHeight;
				copyHeight = oldHeight;
			}
			else
				srcOffset = oldHeight - height;

			var width = TilemapWidth;
			var stride = width << 8;

			void writeLine()
			{
				for (int i = 0; i < stride; i++)
					newMap[dstPos++] = oldMap[srcPos++];
			}

			for (int s = 0; s < dstOffset; s++)
				dstPos += stride;
			for (int s = 0; s < srcOffset; s++)
				srcPos += stride;

			for (int s = 0; s < copyHeight; s++)
				writeLine();

			if (IsEffect)
			{
				RomState.EffectTilemapH = (byte)height;
				RomState.EffectTilemap = newMap;
			}
			else
			{
				RomState.MainTilemapH = (byte)height;
				RomState.MainTilemap = newMap;
			}
		}
		else if (IsEffect)
			RomState.EffectTilemapH = (byte)height;
		else
			RomState.MainTilemapH = (byte)height;

		ReloadTilemap();
	}

	internal static void ChangeWidth(int width)
	{
		var isReverse = width < 0;
		if (isReverse)
			width = -width;

		var oldWidth = TilemapWidth;
		if (oldWidth == width)
			return;

		var newMap = new byte[0x2000];
		var oldMap = TilemapCurrent;
		var srcOffset = 0;
		var dstOffset = 0;
		var srcPos = 0;
		var dstPos = 0;
		var copyWidth = width;

		if (width > oldWidth)
		{
			dstOffset = width - oldWidth;
			copyWidth = oldWidth;
		}
		else
			srcOffset = oldWidth - width;

		var height = TilemapHeight;

		void writeBlock()
		{
			for (int i = 0; i < 0x100; i++)
				newMap[dstPos++] = oldMap[srcPos++];
		}

		for (int y = 0; y < height; y++)
		{
			if (isReverse)
			{
				for (int s = 0; s < dstOffset; s++) dstPos += 0x100;
				for (int s = 0; s < srcOffset; s++) srcPos += 0x100;
			}

			for (int s = 0; s < copyWidth; s++)
				writeBlock();

			if (!isReverse)
			{
				for (int s = 0; s < dstOffset; s++) dstPos += 0x100;
				for (int s = 0; s < srcOffset; s++) srcPos += 0x100;
			}
		}

		if (IsEffect)
		{
			RomState.EffectTilemapW = (byte)width;
			RomState.EffectTilemap = newMap;
		}
		else
		{
			RomState.MainTilemapW = (byte)width;
			RomState.MainTilemap = newMap;
		}

		ReloadTilemap();
	}
}
