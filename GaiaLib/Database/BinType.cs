using System.Text.Json.Serialization;

namespace GaiaLib.Database
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BinType
    {
        Bitmap,
        Tilemap,
        Tileset,
        Palette,
        Sound,
        Music,
        Unknown,
        Meta17,
        Spritemap,
        Assembly,
        Patch,
        Transform
    }
}
