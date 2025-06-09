
using GaiaLib.Types;

namespace GaiaLib.Database
{
    public class DbPart
    {
        internal DbBlock _block;
        internal HashSet<DbPart> Includes;
        internal object ObjectRoot;

        public string Name { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string Struct { get; set; }
        public HexString? Bank { get; set; }
        public string Block { get; set; }

        public bool IsInside(int loc) => loc >= Start && loc < End;
        public bool IsOutside(int loc) => loc < Start || loc >= End;
    }
}
