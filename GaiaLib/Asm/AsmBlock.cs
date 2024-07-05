
namespace GaiaLib.Asm
{
    public class AsmBlock
    {
        public string? Label { get; set; }
        public Location Location { get; set; }
        public int Size { get; set; }

        public List<object> ObjList { get; set; } = [];
    }
}
