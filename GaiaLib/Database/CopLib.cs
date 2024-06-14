using GaiaLib.Asm;

namespace GaiaLib.Database
{
    public class CopLib
    {
        public Dictionary<HexString, CopDef> Codes { get; set; }
        public Dictionary<AddressingMode, string> Formats { get; set; }
    }
}
