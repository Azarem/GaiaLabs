using System.Runtime.InteropServices;

namespace GaiaLib.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta14
    {
        //private readonly byte Command = 0x14;
        public byte Value;

        public Meta14() { }
    }
}
