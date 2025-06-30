using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

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

            //Calculate ASM sizes
            foreach (var asm in asmFiles)
                asm.CalculateSize();

            //Assign locations
            var layout = new RomLayout(allFiles);
            layout.Organize();

            //Rebase assemblies
            foreach (var file in asmFiles)
                file.Rebase();

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
                    var chunkFile = new ChunkFile(filePath, type);

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

                    var oldFile = _root.Files.FirstOrDefault(x =>
                        x.Type == type && x.Name == chunkFile.Name
                    );
                    if (oldFile != null)
                    {
                        chunkFile.Compressed = oldFile.Compressed;
                        chunkFile.Upper = oldFile.Upper ?? false;
                    }

                    if (chunkFile.Compressed != null || type == BinType.Sound)
                        chunkFile.Size += 2;

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
