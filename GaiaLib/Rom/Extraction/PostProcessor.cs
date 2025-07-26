using GaiaLib.Database;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction
{
    internal class PostProcessor
    {
        private readonly ReferenceManager _referenceManager;

        public PostProcessor(BlockReader reader)
        {
            _referenceManager = reader._referenceManager;
        }

        public void Process(DbBlock block)
        {
            if (!string.IsNullOrWhiteSpace(block.PostProcess))
            {
                string[] parts = [];
                var signature = block.PostProcess;
                var index = signature.IndexOf('(');
                if (index > 0)
                {
                    var endIx = signature.IndexOf(')', index);
                    if (endIx < 0)
                        endIx = signature.Length;
                    parts = signature[(index + 1)..endIx].Split(',');
                    signature = signature[..index];
                }
                var method = this.GetType().GetMethod(signature, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    ?? throw new($"Unable to locate postprocess function {signature}");

                method.Invoke(this, [block, .. parts]);
            }
        }

        public void Lookup(DbBlock block, string keyIx, string valueIx)
        {
            var kix = int.Parse(keyIx.Trim());
            var vix = int.Parse(valueIx.Trim());
            var table = block.Parts.First().ObjectRoot as IEnumerable<TableEntry>;
            var tableEntry = table?.First();
            var entries = tableEntry?.Object as IEnumerable<object>;
            var newParts = new List<TableEntry>();
            var newList = new List<object>();

            newParts.Add(new() { Location = tableEntry.Location, Object = newList });
            //RefList[tableEntry.Location] = block.Name;

            int eIx = 1;
            foreach (var entry in entries.OfType<StructDef>())
            {
                int cIx = 0;
                int? key = null;
                object? value = null;
                foreach (var obj in entry.Parts)
                {
                    if (cIx == kix)
                        key = Convert.ToInt32(obj);
                    else if (cIx == vix)
                        value = obj;
                    cIx++;
                }

                if (key == null || value == null)
                    throw new("Could not locate key or value for transform");

                var name = $"entry_{key:X2}";
                var loc = tableEntry.Location + eIx;

                newParts.Add(new() { Location = loc, Object = value });

                //Force labels to match the new name
                _referenceManager._nameTable[loc] = name;

                while (newList.Count <= key)
                    newList.Add((ushort)0);

                newList[key.Value] = $"&{name}";

                eIx++;
            }

            block.Parts.First().ObjectRoot = newParts;
        }
    }
}
