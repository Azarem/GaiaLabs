using GaiaLib.Enum;

namespace GaiaLib.Types;

public class StringWrapper(string str, StringType type, int loc)
{
    public string String = str;

    //public byte[] Data;
    public StringType Type = type;
    public int Marker;
    public int Location = loc;
}
