

using GaiaLib.Asm;

namespace GaiaPacker
{
    internal class AsmBlock
    {
        public string? Label { get; set; }
        public uint? Location { get; set; }
        public int Size { get; set; }

        public List<Op> OpList { get; set; } = [];
    }

    //internal class Op
    //{
    //    public OpCode Code { get; set; }
    //    public object? Operand { get; set; }
    //}
}
