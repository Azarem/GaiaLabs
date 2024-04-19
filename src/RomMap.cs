using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GaiaLabs
{
    public class RomMap
    {
        public ICollection<IData> Blocks { get; set; }

        public DataBlock<StageEntry> StageBlock => Blocks.Single(x => x.Label == "Stage Meta") as DataBlock<StageEntry>;

        public RomMap() { }

        public RomMap(RomLoader loader)
        {
            Initialize();
            foreach (var b in Blocks)
                b.Unpack(loader);

            foreach(var r in loader.Resources)
            {
                //var b = Activator.CreateInstance(typeof(DataBlock<>).MakeGenericType(r.GetType())) as DataBlock;

                Blocks.Add(r.Value);
                r.Value.Unpack(loader);
            }    
        }

        public void Initialize()
        {
            Blocks = new List<IData>(new [] {
                new DataBlock<StageEntry>(0xCD8000, "Stage Meta")
            });
        }

        public void Save(string file)
        {
            using var stream = File.Open(file, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(stream, this);
        }

        public static RomMap Load(string file)
        {
            using var stream = File.Open(file, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<RomMap>(stream);
        }

    }
}
