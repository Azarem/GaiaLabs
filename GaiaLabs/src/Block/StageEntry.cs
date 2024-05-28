using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GaiaLabs
{
    public class StageEntry : DataEntry<MetaEntry>
    {
        public ushort ID { get; set; }

        public StageEntry() : base() { }

        internal override bool HasNext(RomLoader loader) => ID < 0xFF;
        public override bool Unpack(RomLoader loader)
        {
            ID = loader.ReadUInt16();
            return base.Unpack(loader);
        }
    }
}
