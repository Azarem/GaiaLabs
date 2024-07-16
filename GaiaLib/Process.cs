using GaiaLib.Asm;
using GaiaLib.Database;
using System.Globalization;

namespace GaiaLib
{
    public static class Process
    {
        const uint ChunkSize = 0x8000u;

        public class ChunkFile
        {
            public DbFile File;
            public string Path;
            public int Size;
            public Location Location;
            public List<AsmBlock>? Blocks;
            public List<string>? Includes;
            public byte? Bank;

            public ChunkFile(string path, DbFile file)
            {
                File = file;
                Path = path;
            }

            public void Rebase()
            {
                if (Blocks == null)
                    return;

                var loc = Location;
                for (int x = 0, len = Blocks.Count; x < len; x++)
                {
                    var block = Blocks[x];
                    if (x > 0 && block.Label == null)
                        break;
                    block.Location = loc;
                    loc += (uint)block.Size;
                }
            }

            public void Rebase(Location loc)
            {
                Location = loc;
                Rebase();
            }

            public int CalculateSize()
            {
                if (Blocks != null)
                    Size = CalculateSize(Blocks);
                return Size;
            }

            public static int CalculateSize(List<AsmBlock> blocks)
            {
                int size = 0;
                for (int x = 0, len = blocks.Count; x < len; x++)
                {
                    var block = blocks[x];
                    if (x > 0 && block.Label == null)
                        break;
                    size += block.Size;
                }
                return size;
            }

        }

        public static void Repack(string baseDir, string dbFile, Func<ChunkFile, DbRoot, IDictionary<string, Location>, uint> onProcess, Action<uint, object> onTransform)
        {
            var root = DbRoot.FromFile(dbFile);

            var gaps = DiscoverAvailableSpace(root, out var files);
            var sfxFiles = DiscoverSfx(baseDir, root);
            var chunkFiles = DiscoverFiles(baseDir, root, files);
            var patches = DiscoverPatches(baseDir, root, gaps);

            //Process sfx files the same as others
            var allFiles = sfxFiles.Concat(chunkFiles).Concat(patches).ToArray();

            //Assign locations
            MatchChunks(gaps, allFiles);

            var blockLookup = new Dictionary<string, Location>();

            //Rebase assemblies
            //var hasBlocks = allFiles.Where(x => x.Blocks != null).ToArray();
            foreach (var file in allFiles)
                file.Rebase();

            //foreach (var file in hasBlocks.Concat(patches))
            //{
            //    if (file.Includes == null || file.Includes.Count == 0)
            //        continue;

            //    foreach (var inc in file.Includes)
            //    {
            //        var match = patches.FirstOrDefault(x => x.File.Name.Equals(inc, StringComparison.CurrentCultureIgnoreCase))
            //            ?? hasBlocks.FirstOrDefault(x => x.File.Name.Equals(inc, StringComparison.CurrentCultureIgnoreCase));
            //    }
            //}

            //Add other files to lookup
            foreach (var f in root.Files.Except(allFiles.Select(x => x.File)))
                blockLookup[f.Name.ToUpper()] = f.Start;

            //Add files to lookup
            foreach (var f in allFiles.Except(patches))
                blockLookup[f.File.Name.ToUpper()] = f.Location;

            //Process transforms
            foreach (var tr in root.Transforms.Values)
            {
                var name = tr.Name;
                if (!blockLookup.TryGetValue(name.ToUpper(), out var loc))
                {
                    var matchingBlock = allFiles.Where(x => x.File.Type == BinType.Assembly)
                        .SelectMany(x => x.Blocks)
                        .SingleOrDefault(x => name.Equals(x.Label, StringComparison.CurrentCultureIgnoreCase));
                    if (matchingBlock == null)
                        continue;
                    loc = matchingBlock.Location;
                }

                onTransform(tr.Location.Offset, tr.Type switch
                {
                    "%" => (loc.Offset | 0x800000),
                    "*" => (byte)(loc.Bank | 0xC0),
                    "^" => (byte)(loc.Bank | 0x80),
                    _ => (object)(ushort)loc.Offset
                });
            }

            //Generate lookup table
            //var chunkLookup = allFiles.ToDictionary(x => x.File.Name.ToUpper(), x => x.Location);
            //var chunkLookup = allFiles.Select(x => new Tuple<string,Location>(x.File.Name.ToUpper(), x.Location))
            //    .Concat(root.Files
            //        .Except(allFiles.Select(x => x.File))
            //        .Select(x => new ChunkFile { File = x, Location = x.Start }))
            //    .ToDictionary(x => x.File.Name.ToUpper(), x => x.Location);

            //Write file contents
            foreach (var file in allFiles)
                onProcess(file, root, blockLookup);

            //Write patch contents
            //foreach (var grp in patches.GroupBy(x => x.Location))
            //{
            //    var loc = grp.Key.Offset;
            //    foreach (var file in grp)
            //    {
            //        if (loc > 0) file.Location = loc;
            //        var size = onProcess(file, root, blockLookup);
            //        if (loc > 0) loc += size;
            //    }
            //}
        }

        private static List<DbGap> DiscoverAvailableSpace(DbRoot root, out IEnumerable<DbFile> files)
        {
            var sfxStart = root.Sfx.Location.Offset;
            List<DbGap> gaps = new([new DbGap { Start = 0x200000u, End = Location.MaxValue }]);
            files = root.Files.Where(x => x.XRef != null).ToList();
            //var sfxNearest = Location.MaxValue;

            void mergeGap(Location start, Location end)
            {
                //if (sfxStart < end && sfxEnd > start)
                //{
                //    if (start >= sfxStart && (start.Offset & 0x8000u) == 0)
                //        start = sfxEnd;
                //    else if (((end.Offset - 1) & 0x8000u) == 0)
                //        end = sfxStart;
                //}

                //if (start >= sfxEnd && start < sfxNearest)
                //    sfxNearest = start;

                if (end <= start)
                    return;

                for (int i = 0; i < gaps.Count; i++)
                {
                    var g = gaps[i];
                    Location s = g.Start, e = g.End;
                    if (s <= end && e >= start)
                    {
                        gaps.RemoveAt(i);
                        mergeGap(start < s ? start : s, end > e ? end : e);
                        return;
                    }
                }

                gaps.Add(new DbGap { Start = start, End = end });
            }

            //Merge SFX ranges
            var sfxLoc = root.Sfx.Location.Offset;
            var sfxSize = root.Sfx.Size.Value;
            while (sfxSize > 0)
            {
                var s = Math.Min(sfxSize, 0x8000u);
                mergeGap(sfxLoc, sfxLoc + s);
                sfxLoc += 0x10000;
                sfxSize -= s;
            }

            //Merge the file ranges
            foreach (var file in files)
                mergeGap(file.Start, file.End);

            //Gerge gap ranges
            foreach (var space in root.FreeSpace)
                mergeGap(space.Start, space.End);

            //while (sfxEnd < sfxNearest)
            //{
            //    var gapStart = sfxNearest & 0x3F0000u;

            //    if (gapStart <= sfxEnd)
            //    {
            //        mergeGap(sfxEnd, sfxNearest);
            //        break;
            //    }
            //    else
            //    {
            //        mergeGap(gapStart, sfxNearest);
            //        sfxNearest = gapStart - 0x8000u;
            //    }
            //}

            //Split gaps across chunk boundaries
            for (int i = 0; i < gaps.Count; i++)
            {
                var g = gaps[i];
                Location start = g.Start, end = g.End;
                var next = start + (ChunkSize - (start.Offset % ChunkSize));
                if (next == 0u) next--;
                if (end > next)
                {
                    g.End = next;
                    gaps.Add(new DbGap { Start = next, End = end });
                }
            }

            //How much space do we have?
            //var newSize = gaps.Sum(x => x.End.Offset - x.Start.Offset);

            return gaps;
        }

        private static List<ChunkFile> DiscoverSfx(string baseDir, DbRoot root)
        {
            string folder = "sfx", extension = "bin";
            return root.Sfx.Names.Select(name =>
            {
                var path = Path.Combine(baseDir, folder, $"{name}.{extension}");
                return new ChunkFile(path, new() { Type = BinType.Sound, Name = name })
                {
                    Size = (int)(new FileInfo(path).Length + 2)
                };
            }).ToList();

            //return func(sfx.Location, sfx.Names.Select(x => Path.Combine(baseDir, $"{x}.{extension}")));
            //var chunkFiles = new List<ChunkFile>();
            //foreach (var path in sfx.Names.Select(x => Path.Combine(baseDir, $"{x}.{extension}")))
            //    chunkFiles.Add(new() { Path = path, Size = (int)(new FileInfo(path).Length + 2) });

            //return chunkFiles;
        }

        private static List<ChunkFile> DiscoverFiles(string baseDir, DbRoot root, IEnumerable<DbFile> files)
            => files.Select(file =>
            {
                string folder = "misc", extension = "bin";
                switch (file.Type)
                {
                    case BinType.Bitmap: folder = "graphics"; break;
                    case BinType.Palette: folder = "palettes"; extension = "pal"; break;
                    case BinType.Music: folder = "music"; extension = "bgm"; break;
                    case BinType.Tileset: folder = "tilesets"; extension = "set"; break;
                    case BinType.Tilemap: folder = "tilemaps"; extension = "map"; break;
                    case BinType.Meta17: break;
                    case BinType.Spritemap: folder = "spritemaps"; break;
                    case BinType.Assembly: folder = "asm"; extension = "asm"; break;
                        //case BinType.Sound: folder = "sounds"; extension = "sfx"; break;
                };

                var path = Path.Combine(baseDir, folder ?? "", $"{file.Name}.{extension}");// File.Exists(path) ? path : Path.Combine(baseDir, path);
                int size;
                List<AsmBlock>? blocks = null;
                List<string>? includes = null;
                byte? bank = null;
                if (file.Type == BinType.Assembly)
                {
                    using (var inFile = File.OpenRead(path))
                        (blocks, includes, bank) = ParseAssembly(root, inFile, file.Start);

                    size = ChunkFile.CalculateSize(blocks);
                }
                else
                {
                    size = (int)new FileInfo(path).Length;

                    if (file.Compressed != null)
                        size += 2;
                }
                return new ChunkFile(path, file) { Size = size, Blocks = blocks, Includes = includes, Bank = bank };
            }).ToList();

        private static List<ChunkFile> DiscoverPatches(string baseDir, DbRoot root, List<DbGap> gaps)
        {
            string folder = "patches", extension = "asm";
            var patchList = new List<ChunkFile>();

            //var patchReserves = new Dictionary<byte, uint>();

            //uint getOrReserve(byte bank)
            //{
            //    bank &= 0x3F;
            //    if (patchReserves.TryGetValue(bank, out uint location))
            //        return location;

            //    location = ((uint)bank << 16) | 0x8000u;
            //    var gap = gaps.Where(x => x.Start.Bank == bank && x.Start >= location)
            //        .OrderByDescending(x => x.Size)
            //        .FirstOrDefault()
            //        ?? throw new($"Unable to reserve patch space for bank {bank:X2}");

            //    Console.WriteLine($"Reserving space {gap.Start} - {gap.End} for patches");

            //    gaps.Remove(gap);
            //    return patchReserves[bank] = gap.Start;
            //}

            foreach (var file in Directory.GetFiles(Path.Combine(baseDir, folder), $"*.{extension}"))
            {
                byte? bank = null;

                var chunkFile = new ChunkFile(file, new()
                {
                    Type = BinType.Assembly,
                    Name = new FileInfo(file).Name[..^(extension.Length + 1)]
                });

                using (var fileStream = File.OpenRead(file))
                    (chunkFile.Blocks, chunkFile.Includes, chunkFile.Bank) = ParseAssembly(root, fileStream, 0);

                chunkFile.CalculateSize();
                patchList.Add(chunkFile);

                //using (var fileStream = File.OpenRead(file))
                //using (var reader = new StreamReader(fileStream))
                //{
                //    var line = (reader.ReadLine() ?? "").Trim().ToUpper();
                //    if (line.StartsWith("?BANK"))
                //        bank = byte.Parse(line[5..].Trim(), NumberStyles.HexNumber);
                //}

                //patchList.Add(new ChunkFile(file, new()
                //{
                //    Type = BinType.Assembly,
                //    Name = new FileInfo(file).Name[..^(extension.Length + 1)]
                //})
                //{
                //    Bank = bank != null ? (byte)(bank.Value & 0x3F) : null
                //});
            }

            //foreach (var group in patchList.GroupBy(x => x.Bank))
            //{
            //    var bank = group.Key;
            //    int currentRemaining = 0;
            //    Location currentLoc = 0u;
            //    DbGap? currentGap = null;

            //    void reserveSpace()
            //    {
            //        //Gap is full, take it out of processing
            //        if (currentGap != null)
            //        {
            //            gaps.Remove(currentGap);
            //            Console.WriteLine($"Gap full, finding another");
            //        }

            //        currentGap = gaps.Where(x => x.Start.Bank == bank && (x.Start.Offset & 0x8000u) != 0u)
            //            .OrderByDescending(x => x.Size)
            //            .FirstOrDefault()
            //            ?? throw new($"Unable to reserve patch space for bank {bank:X2}");

            //        Console.WriteLine($"Processing space {currentGap.Start} - {currentGap.End} for patches");
            //        //gaps.Remove(gap);

            //        currentRemaining = currentGap.Size;
            //        currentLoc = currentGap.Start;
            //    }

            //    foreach (var file in group)
            //    {
            //        if (bank != null && currentRemaining == 0)
            //            reserveSpace();

            //        using (var fileStream = File.OpenRead(file.Path))
            //            (file.Blocks, file.Includes, file.Bank) = ParseAssembly(root, fileStream, currentLoc);

            //        if (bank != null)
            //        {
            //            var size = file.CalculateSize();
            //            if (size > currentRemaining)
            //            {
            //                reserveSpace();
            //                if (size > currentRemaining)
            //                    throw new($"Not enough available space for patch bank {bank}");
            //                file.Rebase(currentLoc);
            //            }
            //            else
            //                file.Location = currentLoc;

            //            currentLoc += (uint)size;
            //            currentRemaining -= size;
            //        }
            //    }

            //    //Adjust space of used gap
            //    if (currentGap != null)
            //        if (currentRemaining == 0)
            //        {
            //            Console.WriteLine($"Gap full");
            //            gaps.Remove(currentGap);
            //        }
            //        else
            //        {
            //            Console.WriteLine($"Adjusting new gap start {currentLoc}");
            //            currentGap.Start = currentLoc;
            //        }

            //}

            return patchList;
        }

        private static void MatchChunks(IEnumerable<DbGap> gaps, IEnumerable<ChunkFile> files)
        {
            var allFiles = files.OrderByDescending(x => x.Size).ToList();
            var bestResult = new int[0x100];
            var bestSample = new int[0x100];

            foreach (var gap in gaps.OrderBy(x => x.Size).ThenBy(x => x.Start.Offset))
            {
                var remain = gap.Size;
                var count = allFiles.Count;
                if (count == 0)
                    break;
                var smallest = allFiles[^1].Size;
                var isUpper = (gap.Start.Offset & 0x8000) != 0;
                var bank = gap.Start.Bank;
                int bestDepth = 0, bestRemain = remain, bestOffset = 0;

                bool testDepth(int ix, int depth, int remain, bool asmMode)
                {
                    if (ix >= count)
                        return true;

                    for (var i = ix; i < count; i++)
                    {
                        var file = allFiles[i];
                        if (file.Size > remain)
                            continue;

                        if (file.File.Type == BinType.Assembly)
                        {
                            if (!asmMode)
                            {
                                if (!isUpper || file.File.Move != true)
                                    continue;
                            }
                            else if (file.Bank != bank || file.File.Move == true)
                                continue;

                        }
                        else if (asmMode)
                            continue;
                        else if (file.File.Upper == true && !isUpper)
                            continue;

                        var inList = false;
                        for (var y = bestOffset; --y >= 0;)
                            if (bestResult[y] == i)
                            { inList = true; break; }

                        if (inList)
                            continue;

                        bestSample[depth] = i;

                        var newRemain = remain - file.Size;
                        if (newRemain < bestRemain)
                        {
                            bestRemain = newRemain;
                            bestDepth = depth + 1;
                            Array.Copy(bestSample, bestOffset, bestResult, bestOffset, bestDepth - bestOffset);
                        }

                        //Stop when we have an "exact" match
                        if (newRemain < 20)
                            return true;

                        //Stop processing if nothing else can fit
                        if (newRemain < smallest)
                            return false;

                        //Process next iteration and stop if exact
                        if (testDepth(i + 1, depth + 1, newRemain, asmMode))
                            return true;
                    }

                    return false;
                };

                testDepth(0, 0, remain, isUpper);
                if (isUpper && bestRemain >= smallest)
                {
                    bestOffset = bestDepth;
                    testDepth(0, bestDepth, bestRemain, false);
                }

                var position = gap.Start;
                for (int i = 0; i < bestDepth;)
                {
                    var file = allFiles[bestResult[i++]];
                    file.Location = position;
                    //onProcess?.Invoke(file);
                    Console.WriteLine($"  {position}: {file.File.Name}");
                    position += (uint)file.Size;
                }

                Console.WriteLine($"Chunk {gap.Start} - {gap.End} matched with {bestDepth} files {bestRemain:X} remaining");

                if (bestOffset > 0)
                    for (int i = bestDepth; --i >= 0;)
                    {
                        int lastY = 0, lastX = 0, y;
                        for (var x = bestDepth; --x >= 0;)
                            if ((y = bestResult[x]) > lastY)
                            {
                                lastY = y;
                                lastX = x;
                            }

                        bestResult[lastX] = 0;
                        allFiles.RemoveAt(lastY);
                    }
                else
                    for (int i = bestDepth; --i >= 0;)
                        allFiles.RemoveAt(bestResult[i]);

            }

            if (allFiles.Count > 0)
                throw new($"Unable to match {allFiles.Count} files, perhaps there is no room\r\n! {string.Join("\r\n", allFiles.Select(x => x.File.Name))}");

        }


        public static int GetSize(object obj)
        {
            //var obj = objList[index];
            if (obj is Op op)
                return op.Size;
            else if (obj is byte[] arr)
                return arr.Length;
            else if (obj is byte)
                return 1;
            else if (obj is ushort)
                return 2;
            else if (obj is uint)
                return 3;
            else if (obj is string str)
            {
                if (str.Length > 0)
                {
                    switch (str[0])
                    {
                        case '@': return 3;
                        case '%': return 3;
                        case '&': return 2;
                        case '^': return 1;
                    }

                    switch (str)
                    {
                        case "Byte": return 1;
                        case "Word": return 2;
                        case "Offset": return 2;
                        case "Address": return 3;
                    }
                }
            }

            ///TODO: Add parsers for strings
            throw new($"Unable to get size for operand '{obj}'");
        }

        private static char[]
            _whitespace = [' ', '\t'],
            _operators = ['-', '+'],
            _commaspace = [',', ' ', '\t'],
            _addressspace = ['@', '&', '^', '#', '$', '%'],
            _symbolSpace = [',', ' ', '\t', '<', '>', '(', ')', ':', '[', ']', '{', '}', '`', '~', '|'],
            _labelSpace = ['[', '{', '#', '`', '~', '|', ':'],
            _objectspace = ['<', '['];

        public static (List<AsmBlock>, List<string>, byte?) ParseAssembly(DbRoot root, Stream inStream, uint startLoc)//, out IDictionary<string, Location> includes) //, IDictionary<string, Location> chunkLookup)
        {
            using var reader = new StreamReader(inStream);

            var includes = new List<string>();
            var blocks = new List<AsmBlock>();
            AsmBlock current;
            //AsmBlock? target;
            var tags = new Dictionary<string, string?>();
            var memStream = new MemoryStream();
            int ix, bix = 0, lineCount = 0;
            byte? bank = null;
            string? lastStruct = null;

            blocks.Add(current = new AsmBlock { Location = startLoc });

            string? line = "";
            string? getLine()
            {
                //This can happen
                if (line == null)
                    return null;

                //Keep processing what we already have
                if (line.Length > 0)
                    return line;

                Read:
                line = reader.ReadLine();

                //This can happen
                if (line == null)
                    return null;

                lineCount++;

                //Ignore comments
                if ((ix = line.IndexOf("--")) >= 0)
                    line = line[..ix];

                //Ignore comments
                if ((ix = line.IndexOf(';')) >= 0)
                    line = line[..ix];

                //Trim
                line = line.Trim(_commaspace);

                //This can happen
                if (line.Length == 0)
                    goto Read;

                //Make everything case-insensitive (this breaks strings)
                //line = line.ToUpper();

                //Process directives
                if (line[0] == '?')
                {
                    ///TODO: Something later
                    ix = line.IndexOfAny(_commaspace);
                    if (ix < 0) ix = line.Length;
                    var value = line[ix..].TrimStart(_commaspace);

                    switch (line[1..ix].ToUpper())
                    {
                        //This is taken care of by the loader
                        case "BANK":
                            bank = byte.Parse(value, NumberStyles.HexNumber);
                            break;

                        case "INCLUDE":
                            if (value.Length > 0)
                                includes.Add(value);
                            break;
                    }

                    goto Read;
                }

                //Process tags
                if (line[0] == '!')
                {
                    line = line[1..].TrimStart(_commaspace);

                    while (line.Length > 0)
                    {
                        string name = line;
                        string? value = null;
                        if ((ix = line.IndexOfAny(_commaspace)) >= 0)
                        {
                            name = line[..ix];
                            value = line[(ix + 1)..].TrimStart(_commaspace);
                            if ((ix = value.IndexOfAny(_commaspace)) >= 0)
                            {
                                line = value[(ix + 1)..].TrimStart(_commaspace);
                                value = value[..ix];
                            }
                            else
                                line = "";
                        }
                        else
                            line = "";

                        tags[name] = value;
                    }

                    goto Read;
                }

                //Resolve tags
                foreach (var tag in tags)
                    while ((ix = line.IndexOf(tag.Key, StringComparison.CurrentCultureIgnoreCase)) >= 0)
                        line = line[..ix] + tag.Value + line[(ix + tag.Key.Length)..];

                ////Resolve addresses
                //while ((ix = line.IndexOfAny(_addressspace)) >= 0)
                //{
                //    var endIx = line.IndexOfAny(_commaspace, ix);
                //    if (endIx < 0) endIx = line.Length;
                //    var label = line[(ix + 1)..endIx];

                //    if (!chunkLookup.TryGetValue(label, out var loc))
                //    {
                //        target = blocks.FirstOrDefault(x => x.Label?.Equals(label) == true)
                //            ?? throw new($"Unable to find label {label}");
                //        loc = target.Location;
                //    }

                //    var result = (line[ix]) switch
                //    {
                //        '@' => (loc.Offset + 0xC00000u).ToString("X6"),
                //        '&' => (loc.Offset & 0x00FFFFu).ToString("X4"),
                //        '^' => ((loc.Offset >> 16) + 0xC0).ToString("X2"),
                //        _ => throw new("Invalid address operator")
                //    };

                //    line = line[..ix] + '$' + result + line[endIx..];
                //}


                return line;
            }

            void processText(string? struc = null, char? openTag = null)
            {
                var dbs = struc == null ? null
                    : root.Structs.Values.FirstOrDefault(x => x.Name.Equals(struc, StringComparison.CurrentCultureIgnoreCase));

                var parent = dbs?.Parent == null ? null
                    : root.Structs.Values.FirstOrDefault(x => x.Name.Equals(dbs.Parent, StringComparison.CurrentCultureIgnoreCase));

                HexString? desc = parent?.Descriminator;
                HexString? delimiter = dbs?.Delimiter;
                uint memberOffset = 0u, dataOffset = 0u;
                var memberTypes = dbs?.Types;
                var currentType = memberTypes?[memberOffset];

                bool checkDesc()
                {
                    if (desc?.Value == dataOffset)
                    {
                        var obj = dbs.Descriminator.Value.ToObject();
                        current.ObjList.Add(obj);
                        var size = GetSize(obj);
                        current.Size += size;
                        dataOffset += (uint)size;
                        return true;
                    }
                    return false;
                }

                void AdvancePart()
                {
                    if (currentType != null && desc != null)
                        dataOffset += (uint)GetSize(currentType);

                    if (memberTypes != null && memberOffset + 1 < memberTypes.Length)
                        currentType = memberTypes[++memberOffset];
                }

                checkDesc();

                while (line != null)
                {
                    getLine();
                    if (line == null)
                        return;

                    ////Force a stop when we have processed all members of a struct?
                    //if (memberOffset >= dbs?.Parts?.Length)
                    //{
                    //    return;
                    //}

                    if (line.StartsWith("DB"))
                    {
                        string hex = OpCode.HexRegex().Replace(line[2..], "");
                        var data = Convert.FromHexString(hex);
                        current.ObjList.Add(data);
                        current.Size += data.Length;
                        line = "";
                        continue;
                    }


                    string? mnem = null, operand = null;
                    while (line.Length > 0)
                    {
                        if (line[0] == ';')
                        {
                            line = "";
                            break;
                        }

                        //Process strings
                        if (line[0] == '`' || line[0] == '~' || line[0] == '|')
                        {
                            string? str = null;
                            var c = line[0];
                            if ((ix = line.IndexOf(c, 1)) >= 0)
                            {
                                str = line[1..ix];
                                line = line[(ix + 1)..].TrimStart(_commaspace);
                            }
                            else
                            {
                                str = line[1..];
                                line = "";
                            }

                            memStream.Position = 0;
                            memStream.SetLength(0);

                            switch (c)
                            {
                                case '`':
                                    ProcessString(root.WideCommands, root.WideMap, i => (byte)((i & 0x70) << 1 | (i & 0x0F)));
                                    goto Terminate;
                                case '~':
                                    ProcessString(root.WideCommands, root.CharMap, i => (byte)((i & 0x38) << 1 | (i & 0x07)));
                                Terminate:
                                    int last = 0;
                                    if (memStream.Position > 0)
                                    {
                                        memStream.Position--;
                                        last = memStream.ReadByte();
                                    }
                                    if (last != 0xC0 && last != 0xCA)
                                        memStream.WriteByte(0xCA);
                                    break;
                                case '|':
                                    ProcessString(root.StringCommands, null, null);
                                    memStream.WriteByte(0);
                                    break;
                            }

                            void ProcessString(IDictionary<HexString, DbStringCommand> dict, string[]? charMap, Func<byte, byte>? shift)
                            {
                                void processChar(char c)
                                {
                                    if (charMap != null)
                                        for (int i = 0, len = charMap.Length; i < len; i++)
                                        {
                                            var v = charMap[i];
                                            if (c == v[0])
                                            {
                                                memStream.WriteByte(shift((byte)i));
                                                break;
                                            }
                                        }
                                    else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                                        memStream.WriteByte((byte)c);
                                    else
                                    {
                                        var entry = dict.Values.FirstOrDefault(x => x.Types == null && x.Value == c.ToString());
                                        if (entry != null)
                                            memStream.WriteByte((byte)entry.Code.Value);
                                        else
                                            memStream.WriteByte((byte)c); //ASCII
                                    }
                                }

                                for (int x = 0; x < str.Length; x++)
                                {
                                    var c = str[x];
                                    if (c == '[')
                                    {
                                        ix = str.IndexOf(']', x + 1);
                                        var splitChars = new char[] { ':', ',', ' ' };
                                        var parts = str[(x + 1)..ix].Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                                        var cmd = dict.Values.SingleOrDefault(x => x.Value == parts[0]);

                                        if (cmd != null)
                                        {
                                            memStream.WriteByte((byte)cmd.Code.Value);

                                            if (cmd.Types != null)
                                                for (int y = 0, pix = 1; y < cmd.Types.Length; y++, pix++)
                                                {
                                                    switch (cmd.Types[y])
                                                    {
                                                        case MemberType.Byte:
                                                            memStream.WriteByte(byte.Parse(parts[pix], NumberStyles.HexNumber));
                                                            break;

                                                        case MemberType.Word:
                                                            var us = ushort.Parse(parts[pix], NumberStyles.HexNumber);
                                                            memStream.WriteByte((byte)us);
                                                            memStream.WriteByte((byte)(us >> 8));
                                                            break;

                                                        case MemberType.Binary:
                                                            while (pix < parts.Length)
                                                            {
                                                                var ch = byte.Parse(parts[pix], NumberStyles.HexNumber);
                                                                memStream.WriteByte(ch);
                                                                pix++;
                                                            }
                                                            memStream.WriteByte((byte)cmd.Delimiter.Value);
                                                            break;

                                                        case MemberType.Offset:
                                                        case MemberType.Address:
                                                            //Have to keep these for later since we don't have lookups yet
                                                            flushBuffer();
                                                            current.ObjList.Add(parts[pix]);
                                                            current.Size += cmd.Types[y] == MemberType.Offset ? 2 : 3;
                                                            break;
                                                    }
                                                }
                                            else
                                            {

                                            }

                                            x = ix;
                                            continue;
                                        }
                                    }
                                    processChar(c);
                                }

                            }

                            void flushBuffer()
                            {
                                var buffer = memStream.GetBuffer();
                                var size = (int)memStream.Length;
                                if (size > 0)
                                {
                                    var newBuffer = new byte[size];
                                    Array.Copy(buffer, newBuffer, size);
                                    current.ObjList.Add(newBuffer);
                                    current.Size += size;
                                    memStream.Position = 0;
                                    memStream.SetLength(0);
                                }
                            }

                            flushBuffer();
                            AdvancePart();
                            continue;
                        }


                        //Process raw data
                        if (_addressspace.Contains(line[0]))
                        {
                            bool reverse = false;

                            if (line[0] == '#')
                                line = line[1..];

                            if (line[0] == '$')
                            {
                                reverse = true;
                                line = line[1..];
                            }

                            string hex;
                            if ((ix = line.IndexOfAny(_symbolSpace)) >= 0)
                            {
                                hex = line[..ix];
                                line = line[ix..].TrimStart(_commaspace);
                            }
                            else
                            {
                                hex = line;
                                line = "";
                            }

                            if (hex.Length > 0)
                            {
                                //Keep string values of address markers so they can be resolved later
                                if (_addressspace.Contains(hex[0]))
                                {
                                    current.ObjList.Add(hex);
                                    current.Size += GetSize(hex);
                                }
                                else
                                {
                                    var data = Convert.FromHexString(hex);
                                    if (reverse)
                                        for (int x = 0, y = data.Length; --y > x;)
                                        {
                                            var sample = data[x];
                                            data[x++] = data[y];
                                            data[y] = sample;
                                        }

                                    current.ObjList.Add(data);
                                    current.Size += data.Length;
                                }
                            }

                            if (openTag == '[' && lastStruct != null)
                                lastStruct = null;

                            AdvancePart();
                            continue;
                        }


                        if (line[0] == '>')
                        {
                            if (openTag == '<')
                            {
                                line = line[1..].TrimStart(_commaspace);
                                checkDesc();
                            }
                            return;
                        }


                        if (line[0] == ']')
                        {
                            if (openTag != '[')
                                throw new("Missing open tag '['");

                            line = line[1..].TrimStart(_commaspace);

                            if (delimiter == null && lastStruct != null)
                                delimiter = root.Structs.Values.Single(x => x.Name.Equals(lastStruct, StringComparison.CurrentCultureIgnoreCase)).Delimiter;

                            if (delimiter != null)
                            {
                                var obj = delimiter.Value.ToObject();
                                current.ObjList.Add(obj);
                                current.Size += GetSize(obj);
                            }

                            return;
                        }

                        if (line[0] == '}')
                        {
                            if (openTag == '{')
                                line = line[1..].TrimStart(_commaspace);
                            return;
                        }

                        if (line[0] == '[')
                        {
                            line = line[1..].TrimStart(_commaspace);
                            processText(currentType, '[');
                            AdvancePart();
                            continue;
                        }

                        //break;


                        //Process origin tags
                        if (line.StartsWith("ORG"))
                        {
                            line = line[3..].TrimStart(_commaspace);
                            if (line.StartsWith('$'))
                                line = line[1..];

                            string hex;
                            if ((ix = line.IndexOfAny(_commaspace)) >= 0)
                            {
                                hex = line[..ix];
                                line = line[(ix + 1)..].TrimStart(_commaspace);
                            }
                            else
                            {
                                hex = line;
                                line = "";
                            }

                            var location = uint.Parse(hex, NumberStyles.HexNumber);

                            blocks.Add(current = new AsmBlock { Location = location });
                            bix++;

                            continue;
                        }

                        ////Process labels
                        //if ((ix = line.IndexOf(':')) >= 0)
                        //{
                        //    var label = line[..ix].TrimEnd(_commaspace);

                        //    if (label.StartsWith('$'))
                        //        label = label[1..];

                        //    if (label.Length == 6
                        //        && uint.TryParse(label, NumberStyles.HexNumber, null, out var addr)
                        //        && Location.MaxValue >= addr)
                        //    {
                        //        blocks.Add(current = new() { Location = addr });
                        //        bix++;
                        //    }
                        //    else if (label.Length > 0)
                        //    {
                        //        //if (current.Size == 0 && current.Label == null)
                        //        //    current.Label = label;
                        //        //else
                        //        //{
                        //        blocks.Add(current = new() { Location = current.Location + (uint)current.Size, Label = label });
                        //        bix++;
                        //        //}
                        //    }

                        //    line = line[(ix + 1)..].TrimStart(_commaspace);
                        //    continue;
                        //}

                        //Separate instructions into mnemonic and operand parts
                        if ((ix = line.IndexOfAny(_symbolSpace)) > 0)
                        {
                            mnem = line[..ix];
                            operand = line[ix..].TrimStart(_commaspace);

                            //Process object tags
                            if (operand.StartsWith('<'))
                            {
                                line = operand[1..].TrimStart(_commaspace);
                                processText(mnem, '<');
                                if (openTag == '[' && currentType == null)
                                    lastStruct = mnem;
                                mnem = null;
                                continue;
                            }

                            line = operand;
                            //else if (operand.StartsWith('['))
                            //{
                            //    //Take mnem as a label
                            //    var label = line[..ix];
                            //    blocks.Add(current = new() { Label = label, Location = current.Location + (uint)current.Size });
                            //    bix++;

                            //    line = operand[1..].TrimStart(_commaspace);
                            //    processText(null, '[');
                            //    mnem = null;
                            //    continue;
                            //}
                        }
                        else
                        {
                            mnem = line;
                            line = "";
                        }

                        break;
                    }

                    if (mnem?.Length > 0)
                    {
                        //Get list of opcodes from mnemonic
                        if (!OpCode.Grouped.TryGetValue(mnem.ToUpper(), out var codes))
                        {
                            //No mnemonic? Make a label!
                            if (operand != null && _labelSpace.Contains(operand[0]))
                            {
                                if (mnem.StartsWith('$'))
                                    mnem = mnem[1..];

                                if (mnem.Length == 6
                                    && uint.TryParse(mnem, NumberStyles.HexNumber, null, out var addr)
                                    && Location.MaxValue >= addr)
                                {
                                    blocks.Add(current = new() { Location = addr });
                                    bix++;
                                }
                                else if (mnem.Length > 0)
                                {
                                    blocks.Add(current = new() { Location = current.Location + (uint)current.Size, Label = mnem });
                                    bix++;
                                }

                                switch (operand[0])
                                {
                                    case ':':
                                        line = line[1..];
                                        continue;
                                    case '{':
                                    case '[':
                                        line = operand[1..].TrimStart(_commaspace);
                                        processText(currentType, operand[0]);
                                        AdvancePart();
                                        continue;
                                }

                                continue;
                            }
                            throw new($"Unknown instruction line {lineCount}: '{mnem}'");
                        }

                        line = "";

                        //No operand instructions
                        if (string.IsNullOrEmpty(operand))
                        {
                            current.ObjList.Add(new Op { Code = codes.Single(x => x.Size == 1), Operands = [], Size = 1 });
                            current.Size++;
                            continue;
                        }

                        //Do maths before regex processing
                        while ((ix = operand.IndexOfAny(_operators)) >= 0)
                        {
                            var op = operand[ix];

                            int vix = operand.LastIndexOf('$', ix) + 1;
                            //if (vix == 0)
                            //    throw new($"Unable to locate variable for operator line {lineCount}: '{op}'");

                            var value = uint.Parse(operand[vix..ix], NumberStyles.HexNumber);

                            var endIx = operand.IndexOfAny(_symbolSpace, ix + 1);
                            if (endIx < 0) endIx = operand.Length;
                            var number = uint.Parse(operand[(ix + 1)..endIx], NumberStyles.HexNumber);

                            if (op == '-') value -= number;
                            else value += number;

                            var len = (ix - vix) switch
                            {
                                1 or 2 => 2,
                                3 or 4 => 4,
                                5 or 6 => 6,
                                _ => throw new("Invalid size")
                            };

                            operand = operand[..vix] + value.ToString($"X{len}") + operand[endIx..];
                        }

                        OpCode? opCode = null;
                        //int size = 1;
                        foreach (var code in codes)
                        {
                            ///TODO: COP processing
                            if (code.Mnem == "COP")
                            {
                                var splitChars = new char[] { ' ', '\t', ',', '(', ')', '[', ']', '$', '#' };
                                var parts = operand.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                                if (!root.CopLib.Codes.TryGetValue(parts[0], out var cop))
                                    throw new($"Unknown COP command {parts[0]}");

                                current.ObjList.Add(new Op() { Code = code, CopDef = cop, Operands = parts, Size = (byte)(cop.Size + 2) });
                                current.Size += cop.Size + 2;

                                goto Next;
                            }

                            //Keep branch operands until all blocks are processed (for labels)
                            if (code.Mode == AddressingMode.PCRelative || code.Mode == AddressingMode.PCRelativeLong)
                            {
                                opCode = code;
                                //size = code.Size;
                                break;
                            }


                            if (OpCode.AddressingRegex.TryGetValue(code.Mode, out var regex))
                            {
                                var match = regex.Match(operand);
                                if (match.Success)
                                {
                                    operand = match.Groups[1].Value;
                                    opCode = code;
                                    //size = opCode.Size == -2 ? (operand.Length > 2 ? 3 : 2) : opCode.Size;
                                    break;
                                }
                            }
                        }

                        if (operand[0] == '#')
                            operand = operand[1..];

                        if (operand[0] == '$')
                            operand = operand[1..];

                        if (opCode == null)
                        {
                            if ((ix = operand.IndexOfAny(_addressspace)) >= 0)
                            {
                                var eix = operand.IndexOfAny([' ', '\t', ',', ']', ')'], ix);
                                if (eix < 0) eix = operand.Length;
                                operand = operand[ix..eix];
                            }

                            opCode = codes.SingleOrDefault(x => x.Mode == AddressingMode.Immediate)
                                ?? codes.SingleOrDefault(x => x.Mode == AddressingMode.AbsoluteLong)
                                ?? codes.SingleOrDefault(x => x.Mode == AddressingMode.Absolute);

                            //if (operand[0] == '&')
                            //{
                            //    opCode = codes.SingleOrDefault(x => x.Mode == AddressingMode.Immediate)
                            //        ?? codes.SingleOrDefault(x => x.Mode == AddressingMode.Absolute);
                            //    size = (opCode.Size == -2) ? 2 : opCode.Size;
                            //}
                            //else if (operand[0] == '^')
                            //{
                            //    opCode = codes.SingleOrDefault(x => x.Mode == AddressingMode.Immediate);
                            //    size = (opCode.Size == -2) ? 1 : opCode.Size;
                            //}
                            //else // (operand[0] == '@')
                            //{
                            //    opCode = codes.SingleOrDefault(x => x.Mode == AddressingMode.Immediate)
                            //        ?? codes.SingleOrDefault(x => x.Mode == AddressingMode.AbsoluteLong)
                            //        ?? codes.SingleOrDefault(x => x.Mode == AddressingMode.Absolute);

                            //    size = (opCode.Size == -2) ? 2 : opCode.Size;
                            //}

                            //if (mnem[0] == 'J')
                            //{
                            //    opCode = codes.Single(x => x.Mode == AddressingMode.Absolute || x.Mode == AddressingMode.AbsoluteLong);
                            //    operand = $"@{operand}";
                            //    size = opCode.Size;
                            //    //opnd = blocks.FirstOrDefault(x => x.Label?.Equals(operand) == true)
                            //    //    //if (!blocks.Any(x => x.Label?.Equals(operand) == true))
                            //    //    ?? throw new($"Unable to locate target for label line {lineCount}: '{operand}'");
                            //}
                            //else
                            if (opCode == null)
                                throw new($"Unable to determine mode/code line {lineCount}: '{line}'");
                        }

                        int size = opCode.Size;
                        if (opCode.Size == -2)
                            size = (operand[0] == '^' || operand.Length == 2) ? 2 : 3;

                        current.ObjList.Add(new Op() { Code = opCode, Operands = [operand], Size = (byte)size });
                        current.Size += size;
                    //}
                    Next:;
                    }
                }
            }

            processText(null);

            return (blocks, includes, bank);
        }



        //class FileDef
        //{
        //    public Location Loc;
        //    public BinType Type;
        //    public List<Location> Refs = new();
        //}

        //public static unsafe void ReadMetaXrefs(byte* basePtr)
        //{
        //    var fileMap = new Dictionary<Location, FileDef>();

        //    ushort id;
        //    var ptr = basePtr + 0xD8000;
        //    do
        //    {
        //        id = *(ushort*)ptr;
        //        ptr += 2;
        //        while (true)
        //        {
        //            var cmd = *ptr++;
        //            if (cmd == 0) break;

        //            Location loc, xref;
        //            BinType type;

        //            switch (cmd)
        //            {
        //                case 2: ptr += sizeof(Meta2); continue;
        //                case 0x13: ptr += sizeof(Meta13); continue;
        //                case 0x14: ptr += sizeof(Meta14); continue;
        //                case 0x15: ptr += sizeof(Meta15); continue;

        //                case 4:
        //                    var m4 = (Meta4*)ptr;
        //                    loc = (Location)m4->Address;
        //                    type = BinType.Palette;
        //                    xref = (uint)((byte*)(&m4->Address) - basePtr);
        //                    ptr += sizeof(Meta4);
        //                    break;

        //                case 0x11:
        //                    var m11 = (Meta11*)ptr;
        //                    loc = (Location)m11->Address;
        //                    type = BinType.Music;
        //                    xref = (uint)((byte*)(&m11->Address) - basePtr);
        //                    ptr += sizeof(Meta11);
        //                    break;

        //                case 3:
        //                    var m3 = (Meta3*)ptr;
        //                    loc = (Location)m3->Address;
        //                    type = BinType.Bitmap;
        //                    xref = (uint)((byte*)(&m3->Address) - basePtr);
        //                    ptr += sizeof(Meta3);
        //                    break;

        //                case 5:
        //                    var m5 = (Meta5*)ptr;
        //                    loc = (Location)m5->Address;
        //                    type = BinType.Tileset;
        //                    xref = (uint)((byte*)(&m5->Address) - basePtr);
        //                    ptr += sizeof(Meta5);
        //                    break;

        //                case 6:
        //                    var m6 = (Meta6*)ptr;
        //                    loc = (Location)m6->Address;
        //                    type = BinType.Tilemap;
        //                    xref = (uint)((byte*)(&m6->Address) - basePtr);
        //                    ptr += sizeof(Meta6);
        //                    break;

        //                case 0x10:
        //                    var m10 = (Meta10*)ptr;
        //                    loc = (Location)m10->Address;
        //                    type = BinType.Spritemap;
        //                    xref = (uint)((byte*)(&m10->Address) - basePtr);
        //                    ptr += sizeof(Meta10);
        //                    break;

        //                case 0x17:
        //                    var m17 = (Meta17*)ptr;
        //                    loc = (Location)m17->Address;
        //                    type = BinType.Meta17;
        //                    xref = (uint)((byte*)(&m17->Address) - basePtr);
        //                    ptr += sizeof(Meta17);
        //                    break;

        //                default: throw new Exception("Unknown meta type");
        //            }


        //            if (!fileMap.TryGetValue(loc, out FileDef def))
        //                fileMap[loc] = def = new FileDef() { Loc = loc, Type = type };

        //            def.Refs.Add(xref);
        //        }
        //    } while (id < 0xFF);

        //    var stringBuilder = new StringBuilder();

        //    foreach (var entry in fileMap.Values.OrderBy(x => x.Loc))
        //    {
        //        string folder = null, prefix = "", extension = "bin";
        //        switch (entry.Type)
        //        {
        //            case BinType.Bitmap: folder = "graphics"; prefix = "bmp"; break;
        //            case BinType.Palette: folder = "palettes"; prefix = "palette"; extension = "pal"; break;
        //            case BinType.Music: folder = "music"; prefix = "bgm"; extension = "bgm"; break;
        //            case BinType.Tileset: folder = "tilesets"; prefix = "set"; extension = "set"; break;
        //            case BinType.Tilemap: folder = "tilemaps"; prefix = "map"; extension = "map"; break;
        //            //case BinType.Meta17: break;
        //            case BinType.Spritemap: folder = "spritemaps"; prefix = "sprite"; break;
        //                //case BinType.Sound: folder = "sounds"; extension = "sfx"; break;
        //        };
        //        stringBuilder.AppendLine(
        //            $@"{{ ""path"": ""{prefix}_{entry.Loc}.{extension}"", ""type"": ""{entry.Type}"", ""xref"": [ ""{string.Join("\", \"", entry.Refs)}"" ] }},"
        //        );
        //    }

        //    using (var file = File.Create("C:\\project.json"))
        //    using (var writer = new StreamWriter(file))
        //        writer.Write(stringBuilder.ToString());
        //}
    }
}
