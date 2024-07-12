
namespace GaiaLib.Database
{
    public class CopDef
    {
        public string Mnem { get; set; }
        public HexString Code { get; set; }
        public string[] Parts { get; set; }
        public bool Halt { get; set; }
        public byte Size { get; set; }
    }
}
