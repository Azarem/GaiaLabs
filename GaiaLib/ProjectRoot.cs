using GaiaLib.Compression;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Rom;
using GaiaLib.Rom.Extraction;
using GaiaLib.Rom.Rebuild;
using System.Text.Json;

namespace GaiaLib
{
    public class ProjectRoot
    {
        public string Name { get; set; }
        public string RomPath { get; set; }
        public string BaseDir { get; set; }
        public string System { get; set; }
        public string Database { get; set; }
        public string FlipsPath { get; set; }
        public Dictionary<BinType, DbPath> Resources { get; set; }

        //public string ProjectPath { get; set; }
        public string DatabasePath { get; set; }
        public string SystemPath { get; set; }
        public string Compression { get; set; }

        public ICompressionProvider GetCompression()
        {
            var type = Type.GetType($"GaiaLib.Compression.{Compression ?? "QuintetLZ"}");
            return Activator.CreateInstance(type) as ICompressionProvider
                   ?? throw new InvalidOperationException("Could not create compression provider");
        }


        public static ProjectRoot Load(string? path = null)
        {
            path ??= Path.Combine(Environment.CurrentDirectory, "project.json");

            if (File.Exists(path))
            {
                using var file = File.OpenRead(path);

                var project = JsonSerializer.Deserialize<ProjectRoot>(file, DbRoot.JsonOptions)
                    ?? throw new("Error deserializing project file");

                if (string.IsNullOrWhiteSpace(project.BaseDir))
                    project.BaseDir = Directory.GetParent(path).FullName;

                if (string.IsNullOrWhiteSpace(project.Database))
                    project.Database = "us";

                //project.ProjectPath = path;
                project.DatabasePath = Path.Combine(project.BaseDir, "db", project.Database);
                project.SystemPath = Path.Combine(project.BaseDir, "db", "snes");

                return project;
            }

            if (Directory.Exists(path))
            {
                return new ProjectRoot
                {
                    Name = "GaiaLabs",
                    BaseDir = path,
                    FlipsPath = Environment.GetEnvironmentVariable("flips_path"),
                    RomPath = Environment.GetEnvironmentVariable("rom_path"),
                    Database = "us",
                    DatabasePath = Path.Combine(path, "db", "us")
                };
            }

            throw new NotSupportedException("Project file or directory must be valid");
        }

        public void Build()
        {
            using var romWriter = new RomWriter(this);
            romWriter.Repack();
        }

        public async Task<DbRoot> DumpDatabase()
        {
            var root = DbRoot.FromFolder(DatabasePath, SystemPath);
            var data = await File.ReadAllBytesAsync(RomPath);
            root.Paths = Resources;

            //Extract the files from the rom data
            var fileReader = new FileReader(data, root, GetCompression());
            await fileReader.Extract(BaseDir);

            //Extract the sound effects from the rom data
            var sfxReader = new SfxReader(data, root);
            await sfxReader.Extract(BaseDir);

            var blockReader = new BlockReader(data, root);
            blockReader.AnalyzeAndResolve();

            var blockWriter = new BlockWriter(blockReader);
            blockWriter.WriteBlocks(BaseDir);

            return root;
        }

    }

}
