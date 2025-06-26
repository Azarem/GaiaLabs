using GaiaLib.Enum;
using GaiaLib.Rom;
using System.Runtime.InteropServices;

namespace GaiaLib.Types
{
    public struct Address(byte bank, ushort offset)
    {
        public ushort Offset = offset;
        public byte Bank = bank;

        public const int UpperBank = 0x8000;
        public const int DataBankFlag = 0x40;
        public const int FastBankFlag = 0x80;

        public readonly bool IsROM
        {
            get => Space == AddressSpace.ROM;
        }

        public readonly bool IsCodeBank
        {
            get => (Bank & DataBankFlag) == 0;
        }

        public readonly AddressSpace Space
        {
            get
            {
                // Check if bank is in lower half (bank & 0x40 == 0)
                if (IsCodeBank)
                {
                    // Memory map for lower banks
                    return Offset switch
                    {
                        >= RomProcessingConstants.PageSize => AddressSpace.ROM,
                        >= 0x6000 when (Bank & 0x20) != 0 => AddressSpace.SRAM,
                        < 0x2000 => AddressSpace.WRAM,
                        < 0x2100 => AddressSpace.None,
                        < 0x2200 => AddressSpace.System,
                        < 0x3000 => AddressSpace.None,
                        < 0x4100 => AddressSpace.System,
                        < 0x4200 => AddressSpace.None,
                        < 0x4500 => AddressSpace.System,
                        _ => AddressSpace.None,
                    };
                }

                if (Bank == 0x7E || Bank == 0x7F)
                    return AddressSpace.WRAM;

                return AddressSpace.ROM;
            }
        }

        public static implicit operator Address(int addr) => new((byte)(addr >> 16), (ushort)addr);

        public static implicit operator int(Address addr) => ((addr.Bank & 0x3F) << 16) | addr.Offset;

        public override readonly string ToString() => ((Bank << 16) | Offset).ToString("X6");

        public static AddressType TypeFromCode(char code) =>
            code switch
            {
                '^' => AddressType.Bank,
                '&' => AddressType.Offset,
                //'%' => AddressType.Code,
                '@' => AddressType.Address,
                '*' => AddressType.WBank,
                _ => AddressType.Unknown,
            };

        public static char? CodeFromType(AddressType type) =>
            type switch
            {
                AddressType.Bank => '^',
                AddressType.Offset => '&',
                //AddressType.Code => '%',
                AddressType.Address => '@',
                AddressType.WBank => '*',
                _ => null,
            };
    }

    public enum AddressSpace
    {
        None,
        ROM,
        WRAM,
        SRAM,
        System,
    }
}
