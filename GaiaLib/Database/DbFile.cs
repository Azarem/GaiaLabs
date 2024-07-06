namespace GaiaLib.Database
{
    public class DbFile
    {
        public string Name { get; set; }
        public BinType Type { get; set; }
        public Location Start { get; set; }
        public Location End { get; set; }
        public bool? Compressed { get; set; }
        public IEnumerable<string>? XRef { get; set; }
    }
}
