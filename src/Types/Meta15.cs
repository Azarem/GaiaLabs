using System.Runtime.InteropServices;

namespace GaiaLabs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta15
    {
        //private readonly byte Command = 0x15;
        public byte Value;

        public Meta15() { }

        public Meta15(byte value) { Value = value; }
    }
}
