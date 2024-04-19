using Godot;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace GaiaLabs
{
    //[JsonDerivedType(typeof(DataBlock<vstring>), "blk_dialog")]
    public class DataBlock : IData
    {
        public string Label { get; set; }
        public Location Location { get; set; }
        public uint Size { get; set; }
        public IEnumerable<DataEntry> Children { get; set; }
        public IEnumerable<DataReference> References { get; set; }

        public DataBlock() { }

        public DataBlock(Address address, string label, params DataEntry[] children)
        {
            Location = (Location)address;
            Label = label;
            Children = children;
        }

        public virtual bool Unpack(RomLoader loader)
        {
            foreach (var c in Children)
                if (c?.Size == 0)
                {
                    var off = loader._offset;
                    if (c.Unpack(loader))
                        c.Size = loader._offset - off;
                }

            if (Size == 0)
            {
                var last = Children.Last();
                Size = (last.Location + last.Size) - Location;
            }

            return true;
        }
    }

    public class DataBlock<T> : DataBlock where T : DataEntry, new()
    {
        //public ICollection<T> Entries { get; } = new List<T>();
        new public IEnumerable<T> Children { get => base.Children as IEnumerable<T>; set => base.Children = value; }


        public DataBlock() : base() { }

        public DataBlock(Address address, string label, params T[] children)
            : base(address, label, children) { }


        //public IEnumerator<T> GetEnumerator() => Entries.GetEnumerator();

        //IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();

        public override bool Unpack(RomLoader loader)
        {
            if (Children?.Any() != true)
                Children = loader.ReadList<T>(Location);

            return base.Unpack(loader);
        }
    }
}
