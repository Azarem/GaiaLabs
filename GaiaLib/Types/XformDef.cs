using GaiaLib.Enum;

namespace GaiaLib.Types
{
    public class XformDef
    {
        public XformType Type { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public int? KeyIx { get; set; }
        public int? ValueIx { get; set; }
    }
}
