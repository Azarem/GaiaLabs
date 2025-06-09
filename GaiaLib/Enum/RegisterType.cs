using System.Text.Json.Serialization;

namespace GaiaLib.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RegisterType
{
    M,
    X,
    B
}
