
namespace GaiaLib.Sprites
{
    public class SpritePart
    {
        public bool IsLarge { get; set; }
        public byte XOffset { get; set; }
        public byte XOffsetMirror { get; set; }
        public byte YOffset { get; set; }
        public byte YOffsetMirror { get; set; }
        public bool VMirror { get; set; }
        public bool HMirror { get; set; }
        public byte SomeOffset { get; set; }
        public byte PaletteIndex { get; set; }
        public ushort TileIndex { get; set; }
    }
}
