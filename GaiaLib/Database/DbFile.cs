using GaiaLib.Enum;

namespace GaiaLib.Database
{
    public class DbFile
    {
        public string Name { get; set; }
        public BinType Type { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public bool? Compressed { get; set; }
        public bool? Move { get; set; }
        public bool? Upper { get; set; }
    }
}
