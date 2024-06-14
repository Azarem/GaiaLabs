using GaiaLabs.src;
using GaiaLib;
using GaiaLib.Rom;
using System.Collections.Generic;

namespace GaiaLabs
{
    //[JsonDerivedType(typeof(scene_def), "scene")]
    public class DataEntry : IData
    {
        public Location Location { get; set; }
        public uint Size { get; set; }
        public string Label { get; set; }
        //public int Index { get; set; }


        public ICollection<DataReference> References { get; set; } = new List<DataReference>();

        public DataEntry() { }

        //public DataEntry(Address address) { Location = (Location)address; }

        public DataEntry(string label, params DataReference[] references)
        {
            Label = label;
            References = new List<DataReference>(references);
        }

        public DataEntry(Address address, string label, params DataReference[] references)
            : this(label, references)
        {
            Location = (Location)address;
        }

        public virtual bool Unpack(RomLoader loader)
        {
            if (Location == 0u)
                Location = loader._offset; //loader.GetRelative(References.First().Location);

            return true;
        }

        internal virtual bool HasNext(RomLoader loader) => true;// loader.PeekByte() != 0;

    }

    public class DataEntry<T> : DataEntry where T : DataEntry, new()
    {
        public ICollection<T> Children { get; set; }

        public DataEntry() { }

        public DataEntry(Address address, string label, params DataReference[] references)
            : base(address, label, references)
        { }

        public override bool Unpack(RomLoader loader)
        {
            if (!base.Unpack(loader)) return false;

            Children = loader.ReadList<T>();

            return true;
        }
    }
}
