using System.Runtime.InteropServices;

namespace GaiaLib.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta6
    {
        //private readonly byte Command = 0x06;
        public byte Unknown1;
        public Address Address;

        public Meta6() { }
    }
}
