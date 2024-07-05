using System.Runtime.InteropServices;

namespace GaiaLib.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta17
    {
        //private readonly byte Command = 0x17;
        public byte Unknown1;
        public Address Address;
        //public byte Unknown2; //This is unused and is better as a zero

        public Meta17() { }
    }
}
