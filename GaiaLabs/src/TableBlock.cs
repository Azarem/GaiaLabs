
using System.Collections.Generic;

namespace GaiaLabs
{
    //public class TableBlock : DataBlock
    //{
    //    public TableBlock() :base() { }

    //    public TableBlock(Address address, string label, params DataEntry[] children)
    //        : base(address, label, children)
    //    {}
    //}

    public class TableBlock<T> : DataBlock<T> where T : DataEntry, new()
    {
        public TableBlock() : base() { }

        public TableBlock(Address address, string label, params DataReference[] references)
            : base(address, label)
        {
            References = references;
        }

        public override bool Unpack(RomLoader loader)
        {
            Location cur = Location, end, now;
            var children = new List<T>();

            void next() { now = loader.GetRelative(cur); cur += 2; }

            //Find data start (iterate until not zero)
            for (next(); now == 0u; next())
                children.Add(null);

            //Iterate until we reach the first entry (end)
            for (end = now; cur < end; next())
                children.Add(now == 0u ? null : new() { Location = now });

            //Assign property
            Children = children;

            //Continue unpacking
            return base.Unpack(loader);
        }
    }
}
