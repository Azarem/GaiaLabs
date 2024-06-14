using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GaiaLib.Database
{
    public class DbRoot
    {
        public CopLib CopLib { get; set; }
        private IEnumerable<DbBlock> _blocks;
        public IEnumerable<DbBlock> Blocks
        {
            get => _blocks;
            set { _blocks = value; foreach (var p in _blocks) p.Root = this; }
        }

        public IEnumerable<DbFile> Files { get; set; }
        public IEnumerable<DbGap> Gaps { get; set; }
        public IEnumerable<DbOverride> Overrides { get; set; }
        public IDictionary<Location, DbOverride> Returns { get; set; }
        public string[] CharMap { get; set; }
        public string[] WideMap { get; set; }

        public IDictionary<string, DbStruct> Structs { get; set; }
        public IDictionary<HexString, DbStringCommand> StringCommands { get; set; }
        public IDictionary<HexString, DbStringCommand> WideCommands { get; set; }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public static DbRoot FromFile(string dbFile)
        {
            using var file = File.OpenRead(dbFile);
            return JsonSerializer.Deserialize<DbRoot>(file, _jsonOptions);
        }
    }
}
