using System.Text.Json.Serialization;

namespace GaiaLib
{
    public class XformDef
    {
        public XformType Type { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public int? KeyIx { get; set; }
        public int? ValueIx { get; set; }
    }


    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum XformType
    {
        Lookup,
        Replace
    }
}
