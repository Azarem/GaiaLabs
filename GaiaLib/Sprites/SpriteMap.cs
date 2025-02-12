
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
            stream.Position = 0;

            while (!setOffsets.Contains((int)stream.Position))
                setOffsets.Add(getUshort() - 0x4000);

            foreach (var offStart in setOffsets)
            {
                stream.Position = offStart;
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
                stream.Position = offStart;

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

        public void ToStream(Stream stream)
        {
            int pos = FrameSets.Count << 1;

            void writeShort(int val)
            {
                stream.WriteByte((byte)val);
                stream.WriteByte((byte)(val >> 8));
            }

            void writeLoc(int val) => writeShort(val + 0x4000);

            var count = Groups.Count;
            var groupPos = new int[count];

            foreach (var set in FrameSets)
            {
                writeLoc(pos);
                pos += (set.Count << 2) + 2;
            }

            for(int i = 0; i < count; i++)
            {
                groupPos[i] = pos;
                pos += Groups[i].Parts.Count * 7 + 13;
            }

            foreach (var set in FrameSets)
            {
                foreach (var frm in set)
                {
                    writeShort(frm.Duration);
                    writeLoc(groupPos[frm.GroupIndex]);
                }
                writeShort(0xFFFF);
            }

            foreach (var grp in Groups)
            {
                stream.WriteByte(grp.XOffset);
                stream.WriteByte(grp.XOffsetMirror);
                stream.WriteByte(grp.YOffset);
                stream.WriteByte(grp.YOffsetMirror);
                stream.WriteByte(grp.XRecoilHitboxOffset);
                stream.WriteByte(grp.YRecoilHitboxOffset);
                stream.WriteByte(grp.XRecoilHitboxTilesize);
                stream.WriteByte(grp.YRecoilHitboxTilesize);
                stream.WriteByte(grp.XHostileHitboxOffset);
                stream.WriteByte(grp.XHostileHitboxSize);
                stream.WriteByte(grp.YHostileHitboxOffset);
                stream.WriteByte(grp.YHostileHitboxSize);
                stream.WriteByte((byte)grp.Parts.Count);

                foreach (var prt in grp.Parts)
                {
                    stream.WriteByte((byte)(prt.IsLarge ? 1 : 0));
                    stream.WriteByte(prt.XOffset);
                    stream.WriteByte(prt.XOffsetMirror);
                    stream.WriteByte(prt.YOffset);
                    stream.WriteByte(prt.YOffsetMirror);

                    int accum = (prt.VMirror ? 0x8000 : 0)
                        | (prt.HMirror ? 0x4000 : 0)
                        | ((prt.SomeOffset & 0x3) << 12)
                        | ((prt.PaletteIndex & 0x7) << 9)
                        | prt.TileIndex;

                    writeShort(accum);
                }
            }
        }
    }
}
