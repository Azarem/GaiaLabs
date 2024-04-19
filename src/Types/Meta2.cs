using System.Runtime.InteropServices;

namespace GaiaLabs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta2
    {
        //private readonly byte Command = 0x02;
        public byte Value;

        public Meta2() { }

        public Meta2(byte value) { Value = value; }
    }
}
