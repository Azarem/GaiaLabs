using System.Runtime.InteropServices;

namespace GaiaLabs
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
        //public ushort Offset;
        //public byte Bank;

        //public Address Address
        //{
        //    readonly get => new(Bank, Offset);
        //    set { Bank = value.Bank; Offset = value.Offset; }
        //}

        public Meta5() { }

    }
}
