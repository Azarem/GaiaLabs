using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Database
{
    public class DbStringCommand
    {
        public int Key { get; set; }
        public string Value { get; set; }
        public MemberType[] Types { get; set; }
        public int? Delimiter { get; set; }
    }
}
