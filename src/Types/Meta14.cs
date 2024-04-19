using System.Runtime.InteropServices;

namespace GaiaLabs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta14
    {
        //private readonly byte Command = 0x14;
        public byte Value;

        public Meta14() { }

        public Meta14(byte value) { Value = value; }
    }
}
