
using GaiaLib.Enum;

namespace GaiaLib.Asm
{
    public class Registers
    {
        public bool? AccumulatorFlag { get; set; }
        public bool? IndexFlag { get; set; }
        //public byte? ProgramBanK { get; set; }
        public ushort? Direct { get; set; }
        public byte? DataBank { get; set; }
        public ushort? Accumulator { get; set; }
        public ushort? XIndex { get; set; }
        public ushort? YIndex { get; set; }
        public Stack Stack { get; set; } = new();

        public StatusFlags StatusFlags
        {
            get
            {
                StatusFlags flags = 0;
                if (AccumulatorFlag ?? false) flags |= StatusFlags.AccumulatorMode;
                if (IndexFlag ?? false) flags |= StatusFlags.IndexMode;
                return flags;
            }
            set
            {
                AccumulatorFlag = value.HasFlag(StatusFlags.AccumulatorMode);
                IndexFlag = value.HasFlag(StatusFlags.IndexMode);
            }
        }
    }
}
