
namespace GaiaLib.Sprites
{
    public class SpriteMap
    {
        public List<List<SpriteFrame>> FrameSets { get; set; } = [];
        public List<SpriteGroup> Groups { get; set; } = [];

        public static SpriteMap FromStream(Stream stream)
        {
            byte getByte() => (byte)stream.ReadByte();
            ushort getUshort() => (ushort)(stream.ReadByte() | (stream.ReadByte() << 8));

            var spriteMap = new SpriteMap();
            var setOffsets = new HashSet<int>();
            var groupOffsets = new HashSet<int>();
            //var frameSets = new List<List<SpriteFrame>>();
            int position = 0;

            while (!setOffsets.Contains((int)stream.Position))
                setOffsets.Add(getUshort() - 0x4000);

            foreach (var offStart in setOffsets)
            {
                position = offStart;
                List<SpriteFrame> frameList = [];

                do
                {
                    var duration = getUshort();
                    if (duration == 0xFFFF)
                        break;

                    var groupOffset = getUshort() - 0x4000;

                    frameList.Add(new() { Duration = duration, GroupOffset = groupOffset });
                    groupOffsets.Add(groupOffset);
                } while (true);

                spriteMap.FrameSets.Add(frameList);
                //frameSets.Add(frameList);
            }

            int grpIx = 0;
            foreach (var offStart in groupOffsets.OrderBy(x => x))
            {
                position = offStart;

                foreach (var set in spriteMap.FrameSets)
                    foreach (var frame in set)
                        if (frame.GroupOffset == offStart)
                            frame.GroupIndex = (ushort)grpIx;

                var grp = new SpriteGroup
                {
                    XOffset = getByte(),
                    XOffsetMirror = getByte(),
                    YOffset = getByte(),
                    YOffsetMirror = getByte(),
                    XRecoilHitboxOffset = getByte(),
                    YRecoilHitboxOffset = getByte(),
                    XRecoilHitboxTilesize = getByte(),
                    YRecoilHitboxTilesize = getByte(),
                    XHostileHitboxOffset = getByte(),
                    XHostileHitboxSize = getByte(),
                    YHostileHitboxOffset = getByte(),
                    YHostileHitboxSize = getByte()
                };

                var numParts = getByte();
                while (numParts-- > 0)
                {
                    var part = new SpritePart
                    {
                        IsLarge = getByte() != 0,
                        XOffset = getByte(),
                        XOffsetMirror = getByte(),
                        YOffset = getByte(),
                        YOffsetMirror = getByte()
                    };

                    var props = getUshort();
                    part.VMirror = (props & 0x8000) != 0;
                    part.HMirror = (props & 0x4000) != 0;
                    part.SomeOffset = (byte)((props >> 12) & 0x3);
                    part.PaletteIndex = (byte)((props >> 9) & 0x7);
                    part.TileIndex = (ushort)(props & 0x1FF);

                    grp.Parts.Add(part);
                }

                spriteMap.Groups.Add(grp);
                grpIx++;
            }

            return spriteMap;
        }
    }
}
