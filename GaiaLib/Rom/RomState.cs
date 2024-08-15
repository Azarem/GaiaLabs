using GaiaLib.Database;
using System.Drawing;

namespace GaiaLib.Rom
{
    public class RomState
    {
        public readonly byte[] CGRAM = new byte[0x200];
        public readonly byte[] VRAM = new byte[0x10000];

        public static string StripName(string name)
        {
            int ix;
            while ((ix = name.IndexOfAny(Process._addressspace)) >= 0)
                name = name[..ix] + name[(ix + 1)..];
            return name;
        }



        public static RomState FromScene(string baseDir, DbRoot root, string metaFile, int id)
        {
            var res = root.GetPath(BinType.Assembly);
            var filePath = Path.Combine(baseDir, res.Folder, $"{metaFile}.{res.Extension}");

            List<Asm.AsmBlock> blocks;
            List<string> includes;
            byte? bank;
            using (var file = File.OpenRead(filePath))
                (blocks, includes, bank) = Process.ParseAssembly(root, file, 0);

            var label = (blocks[1].ObjList[id] as string).Replace("&", "");
            var block = blocks.First(x => label.Equals(x.Label, StringComparison.CurrentCultureIgnoreCase));
            var list = block.ObjList;

            var state = new RomState();

            Stream getResource(string name, BinType type)
                => File.OpenRead(root.GetResource(baseDir, StripName(name), type));


            for (int ix = 0, count = list.Count; ix < count;)
            {
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
                            var srcOffset = list[ix++];
                            var sizeW = list[ix++];
                            var dstOffset = list[ix++];
                            var layers = list[ix++];
                            var resource = list[ix++];
                        }
                        break;

                    case 0x06:
                        {
                            var layer = list[ix++];
                            var resource = list[ix++];
                        }
                        break;

                    case 0x10:
                        {
                            var sizeB = list[ix++];
                            var dummy1 = list[ix++];
                            var dummy2 = list[ix++];
                            var resource = list[ix++];
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
                            var dstLabel = list[ix++];
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
