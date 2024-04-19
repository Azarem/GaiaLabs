using System.Runtime.InteropServices;

namespace GaiaLabs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta6
    {
        //private readonly byte Command = 0x06;
        public byte Unknown1;
        public Address Address;
        //public ushort Offset;
        //public byte Bank;

        //public Address Address
        //{
        //    readonly get => new(Bank, Offset);
        //    set { Bank = value.Bank; Offset = value.Offset; }
        //}

        public Meta6() { }

    }
}
