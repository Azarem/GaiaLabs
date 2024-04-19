using System.Runtime.InteropServices;

namespace GaiaLabs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Meta3
    {
        //private readonly byte Command = 0x03;
        public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        public Address Address;
        //public ushort Offset;
        //public byte Bank;
        public byte Unknown4;

        //public Address Address
        //{
        //    readonly get => new(Bank, Offset);
        //    set { Bank = value.Bank; Offset = value.Offset; }
        //}

        public Meta3() { }

    }
}
