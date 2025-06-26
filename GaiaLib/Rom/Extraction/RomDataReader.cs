namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Provides low-level ROM data reading functionality
/// </summary>
internal class RomDataReader
{
    public readonly byte[] RomData;
    public int Position;

    public RomDataReader(byte[] romData)
    {
        RomData = romData ?? throw new ArgumentNullException(nameof(romData));
        Position = 0;
    }

    public byte ReadByte()
    {
        return RomData[Position++];
    }

    public sbyte ReadSByte()
    {
        return (sbyte)ReadByte();
    }

    public ushort ReadUShort()
    {
        return (ushort)(ReadByte() | (ReadByte() << 8));
    }

    public short ReadShort()
    {
        return (short)ReadUShort();
    }

    public int ReadAddress()
    {
        return ReadByte() | (ReadByte() << 8) | (ReadByte() << 16);
    }

    public int ReadInt()
    {
        return ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24);
    }

    public int PeekByte()
    {
        return RomData[Position];
    }

    public int PeekShort()
    {
        return RomData[Position] | (RomData[Position + 1] << 8);
    }

    public int PeekAddress()
    {
        return RomData[Position]
            | (RomData[Position + 1] << 8)
            | (RomData[Position + 2] << 16);
    }
} 