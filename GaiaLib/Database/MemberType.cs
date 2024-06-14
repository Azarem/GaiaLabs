using System.Text.Json.Serialization;

namespace GaiaLib.Database
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MemberType
    {
        Byte,
        Word,
        Offset,
        Address,
        Binary,
        String,
        CompString,
        WideString,
        Code
    }
}
