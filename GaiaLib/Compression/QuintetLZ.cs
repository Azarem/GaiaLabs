using GaiaLib.Types;

namespace GaiaLib.Compression
{
    public class QuintetLZ : ICompressionProvider
    {
        public const int DictionarySize = 0x100;

        public byte[] Expand(byte[] srcData, int srcPosition = 0, int srcLen = RomProcessingConstants.PageSize)
        {
            var bitStream = new BitStream(srcData, srcPosition);
            var srcStop = srcPosition + srcLen;

            byte[] dictionary = new byte[DictionarySize];

            //Clear buffer (with spaces)
            for (int i = 0; i < DictionarySize; i++)
                dictionary[i] = 0x20;

            //Initialize positions
            int dictPosition = 0xEF;
            int outPosition = 0;

            //First two bytes is decompressed size
            int dstLen = bitStream.ReadShort();

            //Create output buffer with the expected size
            byte[] outBuffer = new byte[dstLen];

            while (bitStream.Position < srcStop && outPosition < dstLen)
            {
                //If current bit is set, read byte and add to output buffer and dictionary
                if (bitStream.ReadBit())
                {
                    //Read byte from bitstream
                    var sample = (byte)bitStream.ReadByte();

                    //Add byte to output buffer and increment
                    if (outPosition < dstLen)
                        outBuffer[outPosition++] = sample;

                    //Add byte to dictionary and increment
                    dictionary[dictPosition++] = sample;

                    //Wrap dictionary index
                    dictPosition &= 0xFF;
                }
                //If current bit is not set, read sequence from dictionary
                else
                {
                    //read wordIndex and wordLength from bitstream
                    var wordIndex = bitStream.ReadByte();
                    var wordLength = bitStream.ReadNibble() + 2;

                    //Continue while there are remaining bytes to copy
                    while (wordLength-- > 0)
                    {
                        //Read byte at wordIndex and increment
                        var sample = dictionary[wordIndex++];

                        //Wrap dictionary index
                        wordIndex &= 0xFF;

                        //Copy byte to output buffer and increment, but only if there is space
                        if (outPosition < dstLen)
                            outBuffer[outPosition++] = sample;

                        //Add byte to dictionary and increment
                        dictionary[dictPosition++] = sample;

                        //Wrap dictionary index
                        dictPosition &= 0xFF;
                    }
                }
            }

            if (outPosition < dstLen)
                outBuffer = outBuffer[0..outPosition];

            return outBuffer;
        }

        public byte[] Compact(byte[] srcData)
        {
            byte[] dictionary = new byte[0x100];

            //Clear buffer (with spaces?)
            for (int i = 0; i < dictionary.Length; i++)
                dictionary[i] = 0x20;

            byte sample = 0;
            byte bit = 0x80;
            byte offset = 0xEF;
            int srcIx = 0,
                dstIx = 0;

            int srcLen = srcData.Length;
            byte[] outputBuffer = new byte[srcLen];
            byte[] final;

            var bitStream = new BitStream(outputBuffer);


            (byte, byte) getCommand()
            {
                var maxLen = Math.Min(srcLen - srcIx, 17);
                if (maxLen < 2)
                    return (0, 0);

                int startByte = 0,
                    bestLen = 0;
                byte bx = offset;
                for (int i = 0; i < 0x100; i++, bx++)
                {
                    int size = 0;
                    byte bix = bx;
                    while (size < maxLen && dictionary[bix] == srcData[srcIx + size])
                    {
                        bix++;
                        if (bix == offset)
                            bix = bx;
                        size++;
                    }

                    if (size > bestLen)
                    {
                        startByte = bx;
                        bestLen = size;

                        if (bestLen >= maxLen)
                            break;
                    }
                }

                return ((byte)startByte, (byte)bestLen);
            }

            //Write header
            outputBuffer[dstIx++] = (byte)srcLen;
            outputBuffer[dstIx++] = (byte)(srcLen >> 8);

            while (srcIx < srcLen)
            {
                var cmd = getCommand();
                if (cmd.Item2 >= 2)
                {
                    bitStream.WriteBit(false);
                    bitStream.WriteByte(cmd.Item1);
                    bitStream.WriteNibble((byte)(cmd.Item2 - 2));
                    for (int i = 0; i < cmd.Item2; i++)
                        dictionary[offset++] = srcData[srcIx++];
                }
                else
                {
                    bitStream.WriteBit(true);
                    var val = srcData[srcIx++];
                    bitStream.WriteByte(val);
                    dictionary[offset++] = val;
                }
            }

            if (bit != 0x80)
                outputBuffer[dstIx++] = sample;

            final = new byte[dstIx];

            Array.Copy(outputBuffer, final, final.Length);
            return final;
        }
    }
}
