
using Godot;

namespace GaiaLabs
{
    public class TextureEntry : DataEntry
    {
        public ushort Width { get; set; } = 16 * 8;
        public ushort Height { get; set; } = 16 * 8;
        public byte Bpp { get; set; } = 4;
        public byte[] Data { get; set; }
        public override bool Unpack(RomLoader loader)
        {
            if (!base.Unpack(loader))
                return false;

            //TODO: Save data to file instead?
            Data = LZ77.Expand(loader._basePtr + (int)Location.Offset);

            if (Data.Length == 0x4000)
            {
                Height *= 2;
            }

            return true;
        }

        public Image GetImage()
        {
            return ImageConverter.ReadImage(Data, Width, Height, Bpp, 0);
        }
    }
}
