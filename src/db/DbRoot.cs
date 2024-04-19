using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GaiaLabs
{
    public class DbRoot
    {
        public IEnumerable<Dictionary<string, Dictionary<string, Type>>> Types { get; set; }
        public CopLib CopLib { get; set; }
        private IEnumerable<DbBlock> _blocks;
        public IEnumerable<DbBlock> Blocks
        {
            get => _blocks;
            set { _blocks = value; foreach (var p in _blocks) p.Root = this; }
        }


    }

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

    public class DbPart
    {
        internal DbBlock Block;
        internal Op Head;
        internal ICollection<DbPart> Includes;

        public string Name { get; set; }
        public PartType Type { get; set; }
        public Location Start { get; set; }
        public Location End { get; set; }

        public bool IsOutside(Location loc) => loc < Start || loc >= End;
    }

    public class CopLib
    {
        public Dictionary<HexString, CopDef> Codes { get; set; }
        public Dictionary<AddressingMode, string> Formats { get; set; }
    }

    public class CopDef
    {
        public string Mnem { get; set; }
        public HexString Code { get; set; }
        public byte Size { get; set; }
        public string Parts { get; set; }
        public bool Halt { get; set; }

        public string GetFormat()
        {
            var len = Parts.Length;
            var builder = new StringBuilder();
            builder.Append("{0} {1}");
            for (int i = 0; i < len; i++)
            {
                builder.Append(i == 0 ? "{{ " : ", ");
                builder.AppendFormat("{{{0}}}", i + 2);
            }
            builder.Append(" }}");
            return builder.ToString();
        }
    }

    public enum PartType
    {
        Code,
        Subroutine
    }
}
