namespace GaiaLib.Database;

public class DbMnemonic
{
    public int Key { get; set; }
    public required string Value { get; set; }
    public string? Metadata { get; set; }
}
