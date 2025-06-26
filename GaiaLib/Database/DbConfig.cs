using GaiaLib.Enum;

namespace GaiaLib.Database;

public class DbConfig
{
    public int SfxLocation { get; set; }
    public int SfxCount { get; set; }
    //public string[] CharMap { get; set; }
    //public string[] WideMap { get; set; }
    //public string[] AsciiMap { get; set; }
    public string[] AccentMap { get; set; }
    public Dictionary<AddressingMode, string> AsmFormats { get; set; }
    public IEnumerable<DbEntryPoint> EntryPoints { get; set; }
    public IDictionary<BinType, DbPath> Paths { get; set; }
}
