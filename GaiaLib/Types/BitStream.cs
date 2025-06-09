namespace GaiaLib.Types;

public class BitStream(byte[] data, int position = 0)
{
    private const int NibbleMask = 0xF;
    private const int NibblePerfectPosition = 0x08;

    /// <summary>
    /// The binary data to read/write from/to
    /// </summary>
    public byte[] Data { get; private set; } = data;

    /// <summary>
    /// The current read/write position in Data
    /// </summary>
    public int Position { get; set; } = position;

    /// <summary>
    /// The current bit position in the current byte
    /// </summary>
    private int bitFlag = 0x80;

    private int writeSample = 0;

    /// <summary>
    /// Read a byte from the stream
    /// </summary>
    /// <returns>The byte read, or -1 if the end of the data has been reached</returns>
    public int ReadByte()
    {
        //If we've reached the end of the data, return -1
        if (Position >= Data.Length)
            return -1;

        //Read the current byte, and advance the position
        int sample = Data[Position++];

        //If the bit flag is not the most significant bit, we need to include the next byte
        if (bitFlag != 0x80)
        {
            //Include the next byte in the sample
            sample = sample << 8 | Data[Position];

            //Shift the (temporary) bit flag to the right until it is 0
            for (int x = bitFlag; x > 0; x >>= 1)
                //Shift sample for each iteration so it will line up with the byte boundary
                sample >>= 1;
        }

        //Return the sample
        return sample & 0xFF;
    }

    /// <summary>
    /// Read a ushort from the stream (ignores the current bit flag)
    /// </summary>
    /// <returns>The short read, or -1 if the end of the data has been reached</returns>
    public int ReadShort()
    {
        //If end of stream (position would overlap Data.Length), return -1
        if (Position + 1 >= Data.Length)
            return -1;

        //Read sample using two bytes to form a ushort. Advance position by 2
        int value = Data[Position++] | Data[Position++] << 8;

        //Return the value
        return value;
    }

    /// <summary>
    /// Read a bit from the stream
    /// </summary>
    /// <returns>True if the current bit at bitFlag is set, false otherwise</returns>
    public bool ReadBit()
    {
        //If we've reached the end of the data, return false
        if (Position >= Data.Length)
            return false;

        //Read the current bit and test it against current bit flag
        int sample = Data[Position] & bitFlag;

        //Shift the bit flag to the right
        bitFlag >>= 1;

        //If the bit flag is 0, we need to move to the next byte
        if (bitFlag == 0)
        {
            //Reset the bit flag to the most significant bit
            bitFlag = 0x80;
            //Advance the position to the next byte
            Position++;
        }

        //Return true if the bit is set, false otherwise
        return sample != 0;
    }

    /// <summary>
    /// Read a nibble from the stream
    /// </summary>
    /// <returns>The nibble read, or -1 if the end of the data has been reached</returns>
    public int ReadNibble()
    {
        //If we've reached the end of the data, return -1
        if (Position >= Data.Length)
            return -1;

        //Read the current (unshifted) byte sample
        int sample = Data[Position];

        if (bitFlag == NibblePerfectPosition)
        {
            //Reset the bit flag to the most significant bit
            bitFlag = 0x80;
            //Advance the position to the next byte
            Position++;
        }
        else
        {
            //Initialize the bit position to -4 (will be incremented as we shift the bit flag to the right)
            int bitPos = -4;

            //Shift the bit flag to the right until it is 0
            while (bitFlag != 0)
            {
                //Shift the bit flag to the right
                bitFlag >>= 1;
                //Increment the bit position
                bitPos++;
            }

            //If the bit position is less than 0, we need to include the next byte
            if (bitPos < 0)
            {
                //Include the next byte in the sample, and advance the position
                sample = sample << 8 | Data[++Position];

                //Advance the bit position by 8 (will no longer be less than 0)
                bitPos += 8;
            }

            //Shift the sample to the right by the bit position
            sample >>= bitPos;

            //Set the bit flag to the new bit position
            bitFlag = 1 << bitPos - 1;
        }

        //Return the nibble
        return sample & NibbleMask;
    }

    /// <summary>
    /// Write a bit to the stream
    /// </summary>
    /// <param name="set">True if the current bit should be set, false otherwise</param>
    public void WriteBit(bool set)
    {
        //If the bit should be set, set the bit in the write sample
        if (set)
            writeSample |= bitFlag;

        //Shift the bit flag to the right
        bitFlag >>= 1;

        //If the bit flag is 0, we need to move to the next byte
        if (bitFlag == 0)
        {
            //Reset the bit flag to the most significant bit
            bitFlag = 0x80;
            //Write the sample to the data stream
            Data[Position++] = (byte)writeSample;
            //Reset the write sample
            writeSample = 0;
        }
    }

    /// <summary>
    /// Write a byte to the stream
    /// </summary>
    /// <param name="value">The byte to write</param>
    public void WriteByte(int value)
    {
        //If the bit flag is the most significant bit, we can write the byte directly
        if (bitFlag == 0x80)
            Data[Position++] = (byte)value;
        else
        {
            //Shift the value to the left until it is aligned with the bit flag
            for (int x = 1; x <= bitFlag; x <<= 1)
                value <<= 1;

            //Write the value to the data stream
            Data[Position++] = (byte)(writeSample | value >> 8);

            //Update the write sample
            writeSample = value;
        }
    }

    /// <summary>
    /// Write a nibble to the stream
    /// </summary>
    /// <param name="value">The nibble to write</param>
    public void WriteNibble(int value)
    {
        //Mask the value to ensure it is a nibble
        value &= NibbleMask;

        //Upper nibble handling
        if (bitFlag >= 0x10)
        {
            //Determine the shift amount based on the current bit flag
            int shift = bitFlag switch
            {
                0x80 => 4,
                0x40 => 3,
                0x20 => 2,
                0x10 => 1,
            };

            //Update current write sample with value shifted to the correct position
            writeSample |= (byte)(value << shift);

            //Mirror the bit flag onto the lower nibble
            bitFlag >>= 4;
        }
        //Lower nibble handling
        else
        {
            //Determine the shift amount based on the current bit flag
            int shift = bitFlag switch
            {
                0x8 => 0,
                0x4 => 1,
                0x2 => 2,
                0x1 => 3,
                _ => 0,
            };

            //Write current sample with value shifted to correct position
            Data[Position++] = (byte)(writeSample | value >> shift);
            //Update current write sample with value shifted to correct position
            writeSample = (byte)(value << 8 - shift);
            //Mirror the bit flag to the upper to prepare for next write
            bitFlag <<= 4;
        }
    }
}
