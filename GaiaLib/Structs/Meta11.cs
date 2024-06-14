using System.Runtime.InteropServices;

namespace GaiaLib.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta11
    {
        //private readonly byte Command = 0x11;
        public byte Unknown1;
        public byte Unknown2;
        public Address Address;

        public Meta11() { }
    }
}
