using System.Runtime.InteropServices;

namespace GaiaLib
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Address(byte bank, ushort offset)
    {
        public ushort Offset = offset;
        public byte Bank = bank;

        public readonly AddressSpace Space
        {
            get
            {
                if ((Bank & 0x40) == 0)
                {
                    if (Offset >= 0x8000) return AddressSpace.ROM;
                    if (Offset >= 0x6000 && (Bank & 0x20) != 0)
                        return AddressSpace.SRAM;
                    if (Offset < 0x2000) return AddressSpace.WRAM;
                    if (Offset < 0x2100) goto None;
                    if (Offset < 0x2200) return AddressSpace.System;
                    if (Offset < 0x3000) goto None;
                    if (Offset < 0x4000) return AddressSpace.System;
                    if (Offset < 0x4100) return AddressSpace.System;
                    if (Offset < 0x4200) goto None;
                    if (Offset < 0x4500) return AddressSpace.System;
                }
                else if (Bank == 0x7E || Bank == 0x7F)
                    return AddressSpace.WRAM;
                else
                    return AddressSpace.ROM;

                None:
                return AddressSpace.None;
            }
        }

        public readonly uint Position
        {
            get
            {
                if ((Bank & 0x40) == 0)
                {
                    if (Offset >= 0x8000) return ((Bank & 0x3Fu) << 16) | Offset;
                    if (Offset >= 0x6000 && (Bank & 0x20) != 0) return Offset - 0x6000u;
                    if (Offset < 0x2000) return offset;
                    if (Offset < 0x2100) goto Throw;
                    if (Offset < 0x2200) return offset;
                    if (Offset < 0x3000) goto Throw;
                    if (Offset < 0x4000) return offset;
                    if (Offset < 0x4100) return offset;
                    if (Offset < 0x4200) goto Throw;
                    if (Offset < 0x4500) return offset;
                }
                else if (Bank == 0x7E)
                    return Offset;
                else if (Bank == 0x7F)
                    return Offset + 0x10000u;
                else
                    return ((Bank & 0x3Fu) << 16) | Offset;

                Throw:
                throw new("Cannot process NULL address space");
            }
        }


        public static implicit operator Address(uint addr)
            => new((byte)(addr >> 16), (ushort)addr);

        public static implicit operator uint(Address addr)
            => ((uint)addr.Bank << 16) | addr.Offset;

        public override readonly string ToString()
            => ((uint)this).ToString("X6");

        public static AddressType TypeFromCode(char code) => code switch
        {
            '^' => AddressType.Bank,
            '&' => AddressType.Offset,
            '%' => AddressType.Code,
            '@' => AddressType.Data,
            '*' => AddressType.DBank,
            _ => AddressType.Unknown
        };

        public static char? CodeFromType(AddressType type) => type switch
        {
            AddressType.Bank => '^',
            AddressType.Offset => '&',
            AddressType.Code => '%',
            AddressType.Data => '@',
            AddressType.DBank => '*',
            _ => null
        };
    }

    public enum AddressSpace
    {
        None,
        ROM,
        WRAM,
        SRAM,
        System
    }
}
