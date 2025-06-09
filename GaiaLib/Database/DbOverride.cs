using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Database
{
    public class DbOverride
    {
        public int Location { get; set; }
        public RegisterType Register { get; set; }
        public HexString Value { get; set; }
    }
}
