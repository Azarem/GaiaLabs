using System.Runtime.InteropServices;
using System.Text;

namespace GaiaLib.Rom
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public unsafe struct RomHeader
    {
        public fixed byte TitleBytes[21];
        public byte MapMode;
        public byte CartType;
        public byte RomSize;
        public byte RamSize;
        public byte RegionCode;
        public byte DeveloperId;
        public byte Version;
        public ushort ChecksumCompliment;
        public ushort Checksum;

        public ushort Vector1;
        public ushort Vector2;
        public ushort VectorCOP;
        public ushort VectorBRK;
        public ushort VectorAbort;
        public ushort VectorNMI;
        private ushort VectorRES;
        public ushort VectorIRQ;

        public ushort EMVector1;
        public ushort EMVector2;
        public ushort EMVectorCOP;
        private ushort EMVectorBRK;
        public ushort EMVectorAbort;
        public ushort EMVectorNMI;
        public ushort EMVectorRES;
        public ushort EMVectorIRQBRK;

        public string GetTitle() { fixed (byte* ptr = TitleBytes) return Encoding.ASCII.GetString(ptr, 21); }

        private RomHeader* Address { get { fixed (RomHeader* ptr = &this) return ptr; } }

        public int Score(byte* baseAddr)
        {
            //var addr = (byte*)Address;

            ushort resetvector = EMVectorRES;
            ushort checksum = Checksum;
            ushort complement = ChecksumCompliment;

            int score = 0;

            if (resetvector >= 0x8000)
            {
                //ulong resetop_addr = (ulong)resetvector;
                ulong resetop_addr = ((ulong)baseAddr & ~0xffffUL) | (resetvector & 0xffffUL);

                if (((ulong)baseAddr + resetvector) != resetop_addr)
                    return 0;

                byte resetop = baseAddr[resetvector];
                //if (qlread(li, &resetop, sizeof(byte)) != sizeof(byte)) return 0;    //first opcode executed upon reset

                int mapper = (byte)(MapMode & ~0x10);         //mask off irrelevent FastROM-capable bit

                //most likely opcodes
                if (resetop == 0x78  //sei
                || resetop == 0x18  //clc (clc; xce)
                || resetop == 0x38  //sec (sec; xce)
                || resetop == 0x9c  //stz $nnnn (stz $4200)
                || resetop == 0x4c  //jmp $nnnn
                || resetop == 0x5c  //jml $nnnnnn
                ) score += 8;

                //plausible opcodes
                if (resetop == 0xc2  //rep #$nn
                || resetop == 0xe2  //sep #$nn
                || resetop == 0xad  //lda $nnnn
                || resetop == 0xae  //ldx $nnnn
                || resetop == 0xac  //ldy $nnnn
                || resetop == 0xaf  //lda $nnnnnn
                || resetop == 0xa9  //lda #$nn
                || resetop == 0xa2  //ldx #$nn
                || resetop == 0xa0  //ldy #$nn
                || resetop == 0x20  //jsr $nnnn
                || resetop == 0x22  //jsl $nnnnnn
                ) score += 4;

                //implausible opcodes
                if (resetop == 0x40  //rti
                || resetop == 0x60  //rts
                || resetop == 0x6b  //rtl
                || resetop == 0xcd  //cmp $nnnn
                || resetop == 0xec  //cpx $nnnn
                || resetop == 0xcc  //cpy $nnnn
                ) score -= 4;

                //least likely opcodes
                if (resetop == 0x00  //brk #$nn
                || resetop == 0x02  //cop #$nn
                || resetop == 0xdb  //stp
                || resetop == 0x42  //wdm
                || resetop == 0xff  //sbc $nnnnnn,x
                ) score -= 8;

                //at times, both the header and reset vector's first opcode will match ...
                //fallback and rely on info validity in these cases to determine more likely header.

                //a valid checksum is the biggest indicator of a valid header.
                if ((checksum + complement) == 0xffff && (checksum != 0) && (complement != 0)) score += 4;

                ulong addr = (ulong)Address - (ulong)baseAddr;

                if (addr == 0x007fc0 && mapper == 0x20) score += 2;  //0x20 is usually LoROM
                if (addr == 0x00ffc0 && mapper == 0x21) score += 2;  //0x21 is usually HiROM
                if (addr == 0x007fc0 && mapper == 0x22) score += 2;  //0x22 is usually ExLoROM
                if (addr == 0x40ffc0 && mapper == 0x25) score += 2;  //0x25 is usually ExHiROM

                //if (DeveloperId == 0x33) score += 2;             //0x33 indicates extended header
                //if (CartType < 0x08) score++;
                //if (RomSize < 0x10) score++;
                //if (RamSize < 0x08) score++;
                //if (RegionCode < 14) score++;
            }

            if (score < 0)
                score = 0;

            return score;
        }
    }
}
