namespace GaiaLib.Database;

public class DbStringType
{
    public string Name { get; set; }
    public string Delimiter { get; set; }
    public int Terminator { get; set; }
    /// <summary>
    /// The shift function to use when mapping indexes to the character map
    /// </summary>
    public string? ShiftType { get; set; }
    public string[] CharacterMap { get; set; }
    public IDictionary<byte, DbStringCommand> Commands { get; set; }
    public IEnumerable<DbStringLayer> Layers { get; set; }
    public bool GreedyTerminator { get; set; } = false;

    public Func<byte, byte> ShiftDown => DbRoot.ShiftDown[ShiftType ?? ""];
    public Func<byte, byte> ShiftUp => DbRoot.ShiftUp[ShiftType ?? ""];

}
