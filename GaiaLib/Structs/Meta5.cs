using System.Runtime.InteropServices;

namespace GaiaLib.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta5
    {
        //private readonly byte Command = 0x05;
        public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        public byte Unknown4;
        public Address Address;

        public Meta5() { }
    }
}
