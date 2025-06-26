using GaiaLib.Database;
using GaiaLib.Enum;

namespace GaiaLib.Types;

public class StringWrapper(string str, DbStringType type, int loc)
{
    public string String = str;

    //public byte[] Data;
    public DbStringType Type = type;
    public int Marker;
    public int Location = loc;
}
