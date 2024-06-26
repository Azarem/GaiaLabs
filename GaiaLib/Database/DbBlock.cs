﻿
namespace GaiaLib.Database
{
    public class DbBlock
    {
        internal DbRoot Root;

        public string Name { get; set; }

        private IEnumerable<DbPart> _parts;
        public IEnumerable<DbPart> Parts
        {
            get => _parts;
            set { _parts = value; foreach (var p in _parts) p.Block = this; }
        }

        public bool IsOutside(Location loc, out DbPart part)
        {
            if (Parts.All(x => x.IsOutside(loc)))
                foreach (var b in Root.Blocks) //Find chunk this reference belongs to
                    if (b != this && b.IsInside(loc, out part))
                        return true;

            part = null;
            return false;
        }

        public bool IsInside(Location loc, out DbPart part)
        {
            foreach (var p in Parts)
                if (!p.IsOutside(loc))
                { part = p; return true; }

            part = null;
            return false;
        }

        public IEnumerable<DbBlock> GetIncludes()
        {
            return Parts.SelectMany(x => x.Includes).Select(x => x.Block).Distinct();
        }
    }
}
