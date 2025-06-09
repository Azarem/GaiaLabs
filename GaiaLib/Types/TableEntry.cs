
namespace GaiaLib.Types;

public class TableEntry
{
    public int Location { get; set; }

    //public string Name { get; set; }
    public object Object { get; set; }

    public TableEntry() { }

    public TableEntry(int loc)
    {
        Location = loc;
    }
}
