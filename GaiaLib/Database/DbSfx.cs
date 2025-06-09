
using GaiaLib.Types;

namespace GaiaLib.Database
{
    public class DbSfx
    {
        public Location Location { get; set; }
        public HexString Size { get; set; }
        public IEnumerable<string> Names { get; set; }
    }
}
