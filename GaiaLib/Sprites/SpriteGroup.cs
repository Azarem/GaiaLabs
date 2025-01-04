
namespace GaiaLib.Sprites
{
    public class SpriteGroup
    {
        public byte XOffset { get; set; }
        public byte XOffsetMirror { get; set; }
        public byte YOffset { get; set; }
        public byte YOffsetMirror { get; set; }
        public byte XRecoilHitboxOffset { get; set; }
        public byte YRecoilHitboxOffset { get; set; }
        public byte XRecoilHitboxTilesize { get; set; }
        public byte YRecoilHitboxTilesize { get; set; }
        public byte XHostileHitboxOffset { get; set; }
        public byte XHostileHitboxSize { get; set; }
        public byte YHostileHitboxOffset { get; set; }
        public byte YHostileHitboxSize { get; set; }

        public List<SpritePart> Parts = [];
    }
}
