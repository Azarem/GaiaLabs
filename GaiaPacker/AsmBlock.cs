

using GaiaLib.Asm;

namespace GaiaPacker
{
    internal class AsmBlock
    {
        public string? Label { get; set; }
        public uint Location { get; set; }
        public uint Size { get; set; }

        public List<object> ObjList { get; set; } = [];
    }

    //internal class Op
    //{
    //    public OpCode Code { get; set; }
    //    public object? Operand { get; set; }
    //}
}
