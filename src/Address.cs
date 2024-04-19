
using System.Runtime.InteropServices;

namespace GaiaLabs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Address
    {
        public ushort Offset;
        public byte Bank;

        public Address(byte bank, ushort offset)
        {
            Bank = bank;
            Offset = offset;
        }

        public static implicit operator Address(uint addr)
            => new((byte)(addr >> 16), (ushort)addr);

        public static implicit operator uint(Address addr)
            => ((uint)addr.Bank << 16) | addr.Offset;

        public override string ToString()
            => ((uint)this).ToString("X");
    }
}
