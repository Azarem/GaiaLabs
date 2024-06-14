using GaiaLib.Asm;

namespace GaiaLib.Database
{
    public class DbOverride
    {
        public Location Location { get; set; }
        public RegisterType Register { get; set; }
        public HexString Value { get; set; }
    }
}
