
using GaiaLib.Types;

namespace GaiaLib.Asm
{
    public class AsmBlock
    {
        public string? Label { get; set; }
        public int Location { get; set; }
        public int Size { get; set; }
        public bool IsString { get; set; }

        public List<object> ObjList { get; set; } = [];
    }
}
