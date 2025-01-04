
using System.Text.Json.Serialization;

namespace GaiaLib.Sprites
{
    public class SpriteFrame
    {
        public ushort Duration { get; set; }
        public ushort GroupIndex { get; set; }
        [JsonIgnore]
        public int GroupOffset;
    }
}
