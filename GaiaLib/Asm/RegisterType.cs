using System.Text.Json.Serialization;

namespace GaiaLib.Asm
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RegisterType
    {
        M,
        X,
        B
    }
}
