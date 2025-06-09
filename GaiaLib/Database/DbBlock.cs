namespace GaiaLib.Database
{
    public class DbBlock
    {
        internal DbRoot Root;
        internal Dictionary<int, string> Mnemonics = new();

        public string Name { get; set; }
        public bool Movable { get; set; }
        public string Group { get; set; }

        private IEnumerable<DbPart> _parts;
        public IEnumerable<DbPart> Parts
        {
            get => _parts;
            set
            {
                _parts = value;
                foreach (var p in _parts)
                    p._block = this;
            }
        }

        public bool IsOutside(int loc, out DbPart? part)
        {
            if (IsInside(loc, out part))
                return false;

            foreach (var b in Root.Blocks) //Find chunk this reference belongs to
                if (b != this && b.IsInside(loc, out part))
                    return true;

            part = null;
            return true;
        }

        public bool IsInside(int loc, out DbPart? part)
        {
            foreach (var p in Parts)
                if (p.IsInside(loc))
                {
                    part = p;
                    return true;
                }

            part = null;
            return false;
        }

        public IEnumerable<DbBlock> GetIncludes()
        {
            return Parts.SelectMany(x => x.Includes).Select(x => x._block).Distinct();
        }
    }
}
