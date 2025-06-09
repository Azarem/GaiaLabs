namespace GaiaLib.Types;

public class CompressionEntry
{
    public HashSet<string> Strings = new();
    public byte[] Data;
    public int Checksum;
    public int Impact
    {
        get => (Data.Length - 2) * Strings.Count;
    }
} 