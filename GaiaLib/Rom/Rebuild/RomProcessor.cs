using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GaiaLib.Rom.Rebuild
{
    internal class RomProcessor
    {
        private readonly RomWriter _writer;
        private readonly DbRoot _root;
        //public static readonly byte[] _endChars = [0xC0, 0xCA, 0xD1];

        public RomProcessor(RomWriter writer)
        {
            _writer = writer;
            _root = writer._dbRoot;
        }

        public void Repack()
        {
            //Initialize database
            //var root = DbRoot.FromFolder(project.DatabasePath);

            //Update paths from project
            //root.Paths = project.Resources ?? root.Paths;

            //Discover all files to be used
            var allFiles = DiscoverFiles(_writer._projectRoot.BaseDir);

            var patches = allFiles.Where(x => x.Type == BinType.Patch).ToList();
            var stdPatches = patches.Where(x => x.Bank != null).ToList();
            var nullPatches = patches.Where(x => x.Bank == null).ToList();
            var asmFiles = allFiles.Where(x => x.Blocks != null).ToArray();

            ApplyPatches(asmFiles, patches);

            //RebuildDictionary(asmFiles);

            //Calculate sizes
            foreach (var asm in allFiles)
                asm.CalculateSize();

            //Assign locations
            var layout = new RomLayout(allFiles);
            layout.Organize();

            //Rebase assemblies
            foreach (var file in asmFiles)
                file.Rebase();

            //var layoutJson = JsonSerializer.Serialize(new
            //{
            //    files = allFiles.Where(x => x.Size > 0).OrderBy(x => x.Location).Select(x => new
            //    {
            //        name = x.Name,
            //        type = x.Type,
            //        location = x.Location,
            //        size = x.Size,
            //        //bank = x.Bank,
            //        //upper = x.Upper,
            //        //compressed = x.Compressed
            //        //Blocks = x.Blocks?.Select(b => new { b.Label, b.Location, b.Size
            //    })
            //}, new JsonSerializerOptions() { WriteIndented = true });

            //Initialize block lookup
            var blockLookup = new Dictionary<string, int>();

            //Add files to block lookup
            foreach (var f in allFiles)
                blockLookup[f.Name.ToUpper()] = f.Location;

            //Process includes
            foreach (var f in asmFiles)
            {
                f.IncludeLookup = asmFiles
                    .Where(x => f.Includes?.Contains(x.Name.ToUpper()) == true)
                    .SelectMany(x => x.Blocks.Where(x => x.Label != null))
                    .ToDictionary(x => x.Label.ToUpper());

                foreach (var b in f.Blocks.Where(x => x.Label != null))
                    f.IncludeLookup[b.Label.ToUpper()] = b;
            }

            //Write file contents
            foreach (var file in allFiles)
                _writer.WriteFile(file, _root, blockLookup);

            foreach (var file in nullPatches)
                _writer.WriteFile(file, _root, blockLookup);

            var entryBlocks = (from b in asmFiles.Where(x => x.Bank == 0).SelectMany(x => x.Blocks)
                               where b.Label != null
                               join i in _root.EntryPoints on b.Label equals i.Name
                               select new { EntryLocation = i.Location, BlockLocation = b.Location });

            foreach (var e in entryBlocks)
                _writer.WriteTransform(e.EntryLocation, (ushort)e.BlockLocation);

        }

        private static readonly Dictionary<string, int> _sfxLocations = new(){
            { "sfx00", 0x50000 },
            { "sfx01", 0x5065F },
            { "sfx02", 0x506C4 },
            { "sfx03", 0x510A7 },
            { "sfx04", 0x510FA },
            { "sfx05", 0x5144A },
            { "sfx06", 0x5167A },
            { "sfx07", 0x51B0E },
            { "sfx08", 0x51B73 },
            { "sfx09", 0x520FA },
            { "sfx0A", 0x52681 },
            { "sfx0B", 0x53454 },
            { "sfx0C", 0x53471 },
            { "sfx0D", 0x53578 },
            { "sfx0E", 0x53AFF },
            { "sfx0F", 0x54DEB },
            { "sfx10", 0x567A9 },
            { "sfx11", 0x56D54 },
            { "sfx12", 0x57014 },
            { "sfx13", 0x573F4 },
            { "sfx14", 0x607C1 },
            { "sfx15", 0x60FC7 },
            { "sfx16", 0x61B00 },
            { "sfx17", 0x622B5 },
            { "sfx18", 0x62A6A },
            { "sfx19", 0x631B3 },
            { "sfx1A", 0x638FC },
            { "sfx1B", 0x64D1A },
            { "sfx1C", 0x6518A },
            { "sfx1D", 0x6622D },
            { "sfx1E", 0x67828 },
            { "sfx1F", 0x67FB0 },
            { "sfx20", 0x705C7 },
            { "sfx21", 0x7245F },
            { "sfx22", 0x728AB },
            { "sfx23", 0x72D90 },
            { "sfx24", 0x731C1 },
            { "sfx25", 0x73ED7 },
            { "sfx26", 0x742E4 },
            { "sfx27", 0x74DE7 },
            { "sfx28", 0x77CA2 },
            { "sfx29", 0x805FE },
            { "sfx2A", 0x80A65 },
            { "sfx2B", 0x81838 },
            { "sfx2C", 0x81CD5 },
            { "sfx2D", 0x82AE7 },
            { "sfx2E", 0x84BE3 },
            { "sfx2F", 0x85C86 },
            { "sfx30", 0x87281 },
            { "sfx31", 0x9137D },
            { "sfx32", 0x9175D },
            { "sfx33", 0x91D74 },
            { "sfx34", 0x924FC },
            { "sfx35", 0x9292D },
            { "sfx36", 0x947C5 },
            { "sfx37", 0x94C35 },
            { "sfx38", 0x951E0 },
            { "sfx39", 0x9656E },
            { "sfx3A", 0x967DD },
            { "sfx3B", 0x96D64 }
        };

        private List<ChunkFile> DiscoverFiles(string baseDir)
        {
            var chunkFiles = new List<ChunkFile>();
            foreach (BinType type in System.Enum.GetValues<BinType>())
            {
                //Skip transform files, they are only used during unpacking
                if (type == BinType.Transform)
                    continue;

                //Skip the meta17 file, it will be picked up by the unknown type
                if (type == BinType.Meta17)
                    continue;

                var res = _root.GetPath(type);

                foreach (
                    var filePath in Directory.GetFiles(
                        Path.Combine(baseDir, res.Folder),
                        $"*.{res.Extension}",
                        SearchOption.AllDirectories
                    )
                )
                {
                    var chunkFile = new ChunkFile(filePath, type) { Size = (int)new FileInfo(filePath).Length };

                    if (type == BinType.Unknown && chunkFile.Name.StartsWith("meta17"))
                    {
                        chunkFile.Type = BinType.Meta17;
                        chunkFile.Compressed = false;
                    }

                    if (type == BinType.Assembly || type == BinType.Patch)
                    {
                        using var assembler = new Assembler(_root, filePath);
                        (chunkFile.Blocks, chunkFile.Includes, chunkFile.Bank) = assembler.ParseAssembly();
                    }
                    //else if (type == BinType.Palette)
                    //{
                    //    int lastNonzero = 0;
                    //    using var fileStream = File.OpenRead(file);
                    //    for (int i = 0, len = (int)fileStream.Length; i < len; i++)
                    //        if (fileStream.ReadByte() != 0)
                    //            lastNonzero = i;

                    //    chunkFile.Size = (lastNonzero + (0x20 - lastNonzero % 0x20));
                    //}

                    if (chunkFile.Type == BinType.Sound)
                    {
                        chunkFile.Location = _sfxLocations[chunkFile.Name] + 2;
                    }
                    else
                    {
                        var oldFile = _root.Files.FirstOrDefault(x =>
                            x.Type == chunkFile.Type && x.Name == chunkFile.Name
                        );
                        if (oldFile != null)
                        {
                            chunkFile.Compressed = oldFile.Compressed;
                            chunkFile.Upper = oldFile.Upper ?? false;
                            chunkFile.Location = oldFile.Start;
                        }
                        else
                        {
                            var oldAsm = _root.Blocks.FirstOrDefault(x => x.Name == chunkFile.Name);
                            if (oldAsm != null)
                                chunkFile.Location = oldAsm.Parts.First().Start;
                        }
                    }

                    //This is handled in ChunkFile.CalculateSize now
                    //if (chunkFile.Compressed != null || type == BinType.Sound)
                    //    chunkFile.Size += 2;

                    chunkFiles.Add(chunkFile);
                }
            }

            foreach (
                var i in chunkFiles
                    .Where(x => x.Blocks == null)
                    .Select(x =>
                        (x, _root.Files.FirstOrDefault(y => x.Type == y.Type && x.Name == y.Name))
                    )
            )
                if (i.Item2 != null)
                {
                    i.x.Compressed = i.Item2.Compressed;
                    i.x.Upper = i.Item2.Upper ?? false;
                }

            return chunkFiles;
        }

        public static void ApplyPatches(IEnumerable<ChunkFile> asmFiles, IEnumerable<ChunkFile> patches)
        {
            //Replace chunks from patches that matches (what?)
            foreach (var patch in patches.Where(x => x.Includes?.Any() == true))
            {
                ChunkFile? file = null;
                int dstIx = -1;
                var inc = asmFiles.Where(x => patch.Includes.Contains(x.Name.ToUpper())).ToList();
                for (int ix = 0, count = patch.Blocks.Count; ix < count;)
                {
                    var block = patch.Blocks[ix];
                    AsmBlock? match = null;

                    if (block.Label == null)
                        goto Next;

                    foreach (var i in inc)
                        for (int y = 0, yc = i.Blocks.Count; y < yc; y++)
                        {
                            match = i.Blocks[y];
                            if (match.Label == block.Label)
                            {
                                file = i;
                                dstIx = y;
                                goto Next;
                            }
                        }

                    match = null;

                Next:

                    if (match != null)
                        file.Blocks[dstIx++] = block;
                    else if (dstIx >= 0)
                        file.Blocks.Insert(dstIx++, block);
                    else
                    {
                        ix++;
                        continue;
                    }

                    file.Includes.Add(patch.Name.ToUpper());
                    patch.Blocks.RemoveAt(ix);
                    count--;
                }
            }
        }
    }
}
