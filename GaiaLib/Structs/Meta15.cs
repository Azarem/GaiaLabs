using System.Runtime.InteropServices;

namespace GaiaLib.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta15
    {
        //private readonly byte Command = 0x15;
        public byte Value;

        public Meta15() { }
    }
}
