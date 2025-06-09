
namespace GaiaLib.Types;

public class StringSizeComparer : IComparer<string>
{
    public static StringSizeComparer Instance { get; private set; } = new();

    public int Compare(string? x, string? y)
    {
        if (x == null)
            return (y == null) ? 0 : 1;
        else if (y == null)
            return -1;

        return (x.Length > y.Length) ? -1
            : (x.Length < y.Length) ? 1
            : string.Compare(x, y);
    }
}

