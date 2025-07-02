using System.Text.Json;
using GaiaLib.Asm;
using GaiaLib.Enum;
using GaiaLib.Rom;
using GaiaLib.Types;

namespace GaiaLib.Database
{
    public class DbRoot
    {

        public static readonly Dictionary<string, Func<byte, byte>> ShiftDown = new(){
            { "", (x) => x },
            { "h2", (x) => (byte)(((x & 0xE0) >> 1) | x & 0x0F) },
            { "wh2", (x) => (byte)(((x & 0x70) >> 1) | x & 0x07) }
        };

        public static readonly Dictionary<string, Func<byte, byte>> ShiftUp = new(){
            { "", (x) => x },
            { "h2", (x) => (byte)(((x & 0x70) << 1) | x & 0x0F) },
            { "wh2", (x) => (byte)(((x & 0x38) << 1) | x & 0x07) }
        };

        public Dictionary<int, CopDef> CopDef { get; set; }
        public Dictionary<string, CopDef> CopLookup { get; set; }
        //public string[] CharMap { get; set; }
        //public string[] WideMap { get; set; }
        //public IDictionary<int, DbStringCommand> WideCommands { get; set; }
        //public IDictionary<int, DbStringCommand> StringCommands { get; set; }
        public IDictionary<int, string> Mnemonics { get; set; }
        public IDictionary<BinType, DbPath> Paths { get; set; }
        public IDictionary<string, DbStruct> Structs { get; set; }

        public IDictionary<string, DbStringType> StringTypes { get; set; }
        public char[] StringDelimiters { get; set; }
        public IDictionary<char, DbStringType> StringCharLookup { get; set; }

        public IEnumerable<DbFile> Files { get; set; }
        public DbConfig Config { get; set; }

        //public DbSfx Sfx { get; set; }
        //public IEnumerable<DbGap> FreeSpace { get; set; }
        private IEnumerable<DbBlock> _blocks;
        public IEnumerable<DbBlock> Blocks
        {
            get => _blocks;
            set
            {
                _blocks = value;
                foreach (var p in _blocks)
                    p.Root = this;
            }
        }

        //public DbMisc Misc { get; set; }
        public IDictionary<int, DbOverride> Overrides { get; set; }
        //public IDictionary<Location, DbOverride> Returns { get; set; }
        public IDictionary<int, string> Labels { get; set; }
        public IDictionary<int, int> Rewrites { get; set; }
        public IEnumerable<DbEntryPoint> EntryPoints { get; set; }

        public IDictionary<int, OpCode> OpCodes { get; set; }
        public ILookup<string, OpCode> OpLookup { get; set; }



        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        public static DbRoot FromFile(string dbFile)
        {
            using var file = File.OpenRead(dbFile);
            return JsonSerializer.Deserialize<DbRoot>(file, JsonOptions);
        }

        private static T ReadTable<T>(string path)
        {
            using var table = File.OpenRead(path);
            return JsonSerializer.Deserialize<T>(table, JsonOptions);
        }

        public static DbRoot FromFolder(string folderPath, string systemPath)
        {
            var mnemonics = ReadTable<List<DbMnemonic>>(Path.Combine(folderPath, "mnemonics_old.json"));
            var overrides = ReadTable<List<DbOverride>>(Path.Combine(folderPath, "overrides.json"));
            var rewrites = ReadTable<List<DbRewrite>>(Path.Combine(folderPath, "rewrites.json"));
            //var asciiCommands = ReadTable<List<DbStringCommand>>(Path.Combine(folderPath, "asciiCommands.json"));
            //var wideCommands = ReadTable<List<DbStringCommand>>(Path.Combine(folderPath, "stringCommands.json"));
            var blocks = ReadTable<List<DbBlock>>(Path.Combine(folderPath, "blocks.json"));
            var parts = ReadTable<List<DbPart>>(Path.Combine(folderPath, "parts.json"));
            var files = ReadTable<List<DbFile>>(Path.Combine(folderPath, "files.json"));
            var config = ReadTable<List<DbConfig>>(Path.Combine(folderPath, "config.json"));
            var labels = ReadTable<List<DbLabel>>(Path.Combine(folderPath, "labels.json"));
            var structs = ReadTable<List<DbStruct>>(Path.Combine(folderPath, "structs.json"));
            var copdef = ReadTable<List<CopDef>>(Path.Combine(folderPath, "copdef.json"));
            var opCodes = ReadTable<List<OpCode>>(Path.Combine(systemPath, "opCodes.json"));

            var stringTypes = ReadTable<List<DbStringType>>(Path.Combine(folderPath, "stringTypes.json"));
            var stringCommands = ReadTable<List<DbStringCommand>>(Path.Combine(folderPath, "stringCommands.json"));
            var stringLayers = ReadTable<List<DbStringLayer>>(Path.Combine(folderPath, "stringLayers.json"));


            foreach (var block in blocks)
                block.Parts = [.. parts.Where(x => x.Block == block.Name)];

            var cfg = config.FirstOrDefault();

            foreach (var strType in stringTypes)
            {
                strType.Commands = stringCommands.Where(x => x.Set == strType.Name).ToDictionary(x => (byte)x.Key);
                strType.Layers = [.. stringLayers.Where(x => x.Type == strType.Name)];
            }

            return new()
            {
                Mnemonics = mnemonics.ToDictionary(x => x.Key, x => x.Value),
                Overrides = overrides.ToDictionary(x => x.Location),
                Rewrites = rewrites.ToDictionary(x => x.Location, x => x.Value),
                //WideCommands = wideCommands.ToDictionary(x => x.Key),
                //StringCommands = asciiCommands.ToDictionary(x => x.Key),
                Labels = labels.ToDictionary(x => x.Location, x => x.Label),
                Structs = structs.ToDictionary(x => x.Name),
                Blocks = blocks,
                Files = files,
                CopDef = copdef.ToDictionary(x => x.Code),
                CopLookup = copdef.ToDictionary(x => x.Mnem),
                Config = cfg,
                OpCodes = opCodes.ToDictionary(x => x.Code),
                OpLookup = opCodes.ToLookup(x => x.Mnem),
                EntryPoints = cfg.EntryPoints,
                Paths = cfg.Paths,
                StringTypes = stringTypes.ToDictionary(x => x.Name),
                StringDelimiters = [.. stringTypes.Select(x => x.Delimiter[0])],
                StringCharLookup = stringTypes.ToDictionary(x => x.Delimiter[0])
            };
        }

        public DbPath GetPath(BinType type)
        {
            if (!Paths.TryGetValue(type, out var path))
                path = Paths[BinType.Unknown];
            return path;
        }

        public string GetResource(string baseDir, string name, BinType type)
        {
            var res = GetPath(type);
            return Path.Combine(baseDir, res.Folder, $"{name}.{res.Extension}");
        }
    }
}
