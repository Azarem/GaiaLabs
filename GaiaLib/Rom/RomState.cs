using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Rom;
using GaiaLib.Rom.Rebuild;
using GaiaLib.Sprites;
using GaiaLib.Types;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace GaiaLib.Rom
{
    public class RomState
    {
        // 0 0 0 0 0 0 0 1  0x01 = Tile Page
        // 0 0 0 0 0 0 1 0  0x02 = ??
        // 0 0 0 1 1 1 0 0  0x1C = Palette (Color) Offset
        // 0 0 1 0 0 0 0 0  0x20 = High Priority
        // 0 1 0 0 0 0 0 0  0x40 = Horizontal Mirror
        // 1 0 0 0 0 0 0 0  0x80 = Vertical Mirror




        public readonly byte[] CGRAM = new byte[0x200];
        public readonly byte[] VRAM = new byte[0x10000];
        //public readonly byte[] WRAM = new byte[0x20000];
        public readonly byte[] MainTileset = new byte[0x800];
        public readonly byte[] EffectTileset = new byte[0x800];
        public string? MainTilesetPath, EffectTilesetPath;
        public byte[] MainTilemap = new byte[0x2000];
        public byte MainTilemapW, MainTilemapH;
        public string? MainTilemapPath;
        public byte[] EffectTilemap = new byte[0x2000];
        public byte EffectTilemapW, EffectTilemapH;
        public string? EffectTilemapPath;

        public static SpriteMap? SpriteMap = null;
        public static string SpriteMapPath = null;

        public static string StripName(string name)
        {
            while (RomProcessingConstants.AddressSpace.Contains(name[0]))
                name = name[1..];

            return name;
        }



        public static RomState FromScene(string baseDir, DbRoot root, string metaFile, int id)
        {
            var res = root.GetPath(BinType.Assembly);
            var filePath = Path.Combine(baseDir, res.Folder, $"{metaFile}.{res.Extension}");

            List<AsmBlock> blocks;
            HashSet<string> includes;
            byte? bank;
            using (var assembler = new Assembler(root, filePath))
            {
                (blocks, includes, bank) = assembler.ParseAssembly();
            }

            var label = (blocks[1].ObjList[id] as string).Replace("&", "");
            var block = blocks.First(x => label.Equals(x.Label, StringComparison.CurrentCultureIgnoreCase));
            var list = block.ObjList;

            var state = new RomState();

            SpriteMap = null;

            FileStream getResource(string name, BinType type)
                => File.OpenRead(root.GetResource(baseDir, StripName(name), type));


            for (int ix = 0, count = list.Count; ix < count;)
            {
            Top:
                switch ((byte)list[ix++])
                {
                    case 0x02:
                        {
                            var ppuIx = list[ix++];
                        }
                        break;

                    case 0x03:
                        {
                            var srcOffset = (byte)list[ix++] << 9;
                            var sizeW = (byte)list[ix++] << 9;
                            var dstOffset = (byte)list[ix++] << 9;
                            var resource = list[ix++];
                            var isSprites = (byte)list[ix++] != 0;

                            int size = sizeW - srcOffset;
                            int sIx = 0, dIx = dstOffset + (isSprites ? 0x8000 : 0x4000);

                            using var file = getResource(resource.ToString(), BinType.Bitmap);

                            file.Position = srcOffset;
                            while (sIx++ < size)
                                state.VRAM[dIx++] = (byte)file.ReadByte();
                        }
                        break;

                    case 0x04:
                        {
                            var srcOffset = (byte)list[ix++];
                            var sizeW = (byte)list[ix++];
                            var dstOffset = (byte)list[ix++];
                            var resource = list[ix++];

                            int offset = srcOffset << 1;
                            int size = (sizeW << 1) - offset;
                            int sIx = 0, dIx = dstOffset << 1;

                            using var file = getResource(resource.ToString(), BinType.Palette);

                            file.Position = offset;
                            while (sIx++ < size)
                                state.CGRAM[dIx++] = (byte)file.ReadByte();
                        }
                        break;

                    case 0x05:
                        {
                            var srcOffset = (byte)list[ix++] << 2;
                            var sizeW = (byte)list[ix++] << 6;
                            var dstOffset = (byte)list[ix++] << 2;
                            var layers = (byte)list[ix++];
                            var resource = list[ix++];

                            using var file = getResource(resource.ToString(), BinType.Tileset);

                            void copy(byte[] buffer)
                            {
                                file.Position = srcOffset;
                                var dix = dstOffset;
                                for (int i = sizeW - srcOffset; i-- > 0;)
                                    buffer[dix++] = (byte)file.ReadByte();
                            };

                            if ((layers & 1) != 0)
                            {
                                copy(state.MainTileset);
                                state.MainTilesetPath = file.Name;
                            }

                            if ((layers & 2) != 0)
                            {
                                copy(state.EffectTileset);
                                state.EffectTilesetPath = file.Name;
                            }
                        }
                        break;

                    case 0x06:
                        {
                            var layer = (byte)list[ix++];
                            var resource = list[ix++];

                            using var file = getResource(resource.ToString(), BinType.Tilemap);

                            var width = file.ReadByte();
                            var height = file.ReadByte();
                            var calcSize = (width * height) << 8;
                            byte[] buffer;//= (layer & 1) != 0 ? state.MainTilemap : state.EffectTilemap;

                            if ((layer & 1) != 0)
                            {
                                buffer = state.MainTilemap;
                                state.MainTilemapW = (byte)width;
                                state.MainTilemapH = (byte)height;
                                state.MainTilemapPath = file.Name;
                            }
                            else
                            {
                                buffer = state.EffectTilemap;
                                state.EffectTilemapW = (byte)width;
                                state.EffectTilemapH = (byte)height;
                                state.EffectTilemapPath = file.Name;
                            }

                            for (int i = 0, size = Math.Min(0x2000, Math.Min((int)file.Length - 2, calcSize)); i < size;)
                                buffer[i++] = (byte)file.ReadByte();
                        }
                        break;

                    case 0x10:
                        {
                            var sizeW = list[ix++];
                            var dummy = list[ix++];
                            var resource = list[ix++];

                            using var file = getResource(resource.ToString(), BinType.Spritemap);

                            SpriteMap = SpriteMap.FromStream(file);
                            SpriteMapPath = file.Name;
                        }
                        break;

                    case 0x11:
                        {
                            var musicId = list[ix++];
                            var roomGroup = list[ix++];
                            var resource = list[ix++];
                        }
                        break;

                    case 0x13:
                        {
                            var flagIx = list[ix++];
                            var dstLabel = list[ix++];
                        }
                        break;

                    case 0x14:
                        {
                            var jmpLabel = list[ix++];
                        }
                        break;

                    case 0x15:
                        {
                            var dstLabel = (byte)list[ix++];

                            for (int bix = 2; bix < blocks.Count; bix++)
                            {
                                var bList = blocks[bix].ObjList;
                                for (int pix = 0; pix < bList.Count;)
                                {
                                    switch ((byte)bList[pix++])
                                    {
                                        case 0x02: pix++; break;
                                        case 0x03: case 0x05: pix += 5; break;
                                        case 0x04: case 0x10: pix += 4; break;
                                        case 0x11: pix += 3; break;
                                        case 0x06: case 0x13: case 0x17: pix += 2; break;
                                        case 0x14:
                                            if (dstLabel == (byte)bList[pix++])
                                            {
                                                block = blocks[bix];
                                                list = block.ObjList;
                                                count = list.Count;
                                                ix = pix;
                                                goto Top;
                                            }
                                            break;
                                    }
                                }
                            }

                        }
                        break;

                    case 0x17:
                        {
                            var unknown = list[ix++];
                            var resource = list[ix++];
                        }
                        break;

                    default:
                        ix = count;
                        break;
                }
            }

            return state;
        }
    }
}
