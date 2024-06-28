
namespace GaiaLib.Database
{
    public class DbGap
    {
        public Location Start { get; set; }
        public Location End { get; set; }
        public int Size { get => (End == Location.MaxValue ? (int)(End.Offset + 1) : (int)End.Offset) - (int)Start.Offset; }
    }
}
