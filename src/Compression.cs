
using System;

namespace GaiaLabs
{
    public static unsafe class Compression
    {
        public static unsafe byte[] Expand(nint srcData, int srcLen = 0x8000)
        {
            byte* ptr = (byte*)srcData, stop = ptr + srcLen;
            byte[] buffer = new byte[0x100];

            //Clear buffer (with spaces?)
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0x20;

            var bit = 0x80;
            var cmd = *ptr & bit;
            byte offset = 0xEF;
            int outIx = 0;

            //First two bytes is decompressed size
            int dstLen = *(ushort*)ptr;
            ptr += 2;

            byte[] outBuffer = new byte[dstLen];

            bool getCmd()
            {
                int sample = *ptr & bit;
                bit >>= 1;
                if (bit == 0)
                {
                    bit = 0x80;
                    ptr++;
                }
                return sample != 0;
            }

            byte getByte()
            {
                int sample = *ptr++;
                if (bit < 0x80)
                {
                    sample = (sample << 8) | *ptr;
                    for (int x = bit; x > 0; x >>= 1)
                        sample >>= 1;
                }
                return (byte)sample;
            }

            byte getNibble()
            {
                int sample = *ptr;

                if (bit == 0x08)
                {
                    bit = 0x80;
                    ptr++;
                }
                else
                {
                    int target = -4;
                    while (bit != 0)
                    {
                        bit >>= 1;
                        target++;
                    }

                    if (target < 0)
                    {
                        sample = (sample << 8) | *++ptr;
                        target += 8;
                    }
                    sample >>= target;
                    bit = 1 << (target - 1);
                }

                return (byte)(sample & 0xF);
            }

            void writeByte(byte val)
            {
                if (outIx < dstLen)
                    outBuffer[outIx++] = val;
                buffer[offset++] = val;
            }

            while (ptr < stop && outIx < dstLen)
            {
                if (getCmd())
                    writeByte(getByte());
                else
                {
                    for (byte o = getByte(), i = (byte)(getNibble() + 2); i > 0; i--, o++)
                        writeByte(buffer[o]);
                }
            }

            if (outIx < dstLen)
                outBuffer = outBuffer[0..outIx];

            return outBuffer;
        }

        public static unsafe byte[] Compact(byte[] srcData)
        {
            byte[] dictionary = new byte[0x100];

            //Clear buffer (with spaces?)
            for (int i = 0; i < dictionary.Length; i++)
                dictionary[i] = 0x20;

            byte sample = 0;
            byte bit = 0x80;

            byte* ptr;

            void writeCmd(bool cmd)
            {
                if (cmd)
                    sample |= bit;

                bit >>= 1;
                if (bit == 0)
                {
                    bit = 0x80;
                    *ptr++ = sample;
                    sample = 0;
                }
            }

            void writeByte(int cmd)
            {
                if (bit == 0x80)
                    *ptr++ = (byte)cmd;
                else
                {
                    for (int x = 1; x <= bit; x <<= 1)
                        cmd <<= 1;

                    *ptr++ = (byte)(sample | (cmd >> 8));
                    sample = (byte)cmd;
                }
            }

            void writeNibble(byte cmd)
            {
                cmd &= 0xF;

                byte shift = 2;
                switch (bit)
                {
                    case 0x80: shift = 4; goto case 0x20;
                    case 0x40: shift = 3; goto case 0x20;
                    case 0x10: shift = 1; goto case 0x20;
                    case 0x20:
                        sample |= (byte)(cmd << shift);
                        bit >>= 4;
                        break;

                    case 0x8: shift = 0; goto case 0x2;
                    case 0x4: shift = 1; goto case 0x2;
                    case 0x1: shift = 3; goto case 0x2;
                    case 0x2:
                        *ptr++ = (byte)(sample | (cmd >> shift));
                        sample = (byte)(cmd << (8 - shift));
                        bit <<= 4;
                        break;
                }

            }

            return null;
        }
    }
}
