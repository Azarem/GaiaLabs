using System.Text.Json.Serialization;

namespace GaiaLib.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MemberType
{
    Byte,
    Word,
    Offset,
    Address,
    Binary,
    //String,
    //CharString,
    //WideString,
    Code
}
