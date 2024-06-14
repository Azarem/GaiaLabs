
namespace GaiaLib.Database
{
    public class DbPart
    {
        internal DbBlock Block;
        internal HashSet<DbPart> Includes;
        internal object ObjectRoot;

        public string Name { get; set; }
        public Location Start { get; set; }
        public Location End { get; set; }
        public string Struct { get; set; }

        public bool IsInside(Location loc) => loc >= Start && loc < End;
        public bool IsOutside(Location loc) => loc < Start || loc >= End;
    }
}
