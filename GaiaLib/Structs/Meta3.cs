using System.Runtime.InteropServices;

namespace GaiaLib.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta3
    {
        //private readonly byte Command = 0x03;
        public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        public Address Address;
        public byte Unknown4;

        public Meta3() { }
    }
}
