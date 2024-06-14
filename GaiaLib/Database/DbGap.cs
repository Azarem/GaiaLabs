
namespace GaiaLib.Database
{
    public class DbGap
    {
        public Location Start { get; set; }
        public Location End { get; set; }
        public int Size { get => (int)End.Offset - (int)Start.Offset; }
    }
}
