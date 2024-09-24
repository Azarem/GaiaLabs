using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Rom;
using System.Globalization;
using System.Reflection;

namespace GaiaLib
{
    public static class Process
    {
        const uint ChunkSize = 0x8000u;

        private static readonly string[] _dictionaries = ["dictionary_01EBA8", "dictionary_01F54D"];//, "templates_01CA95"];
        private static readonly byte[] _dictCommands = [0xD6, 0xD7];//, 0xC2];

        //private const string _dict1Name = "dictionary_01EBA8";
        //private const string _dict2Name = "dictionary_01F54D";
        //private const string _dict3Name = "templates_01CA95";
        public class ChunkFile
        {
            //public DbFile File;
            public string Path;
            public string Name;
            public int Size;
            public Location Location;
            public List<AsmBlock>? Blocks;
            public HashSet<string>? Includes;
            public Dictionary<string, Location> IncludeLookup;
            public byte? Bank;
            public BinType Type;
            public bool? Compressed;
            public bool Upper;

            public ChunkFile(string path, BinType type)//, DbFile file)
            {
                //File = file;
                Path = path;
                Name = System.IO.Path.GetFileNameWithoutExtension(path);
                Type = type;
                Size = (int)new FileInfo(path).Length;

                Compressed = type switch
                {
                    BinType.Bitmap or BinType.Tilemap or BinType.Tileset or BinType.Spritemap or BinType.Meta17 => false,
                    _ => null
                };
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

        public class StringEntry
        {
            public AsmBlock Block;
            public int Index;
            public int Size;
            public byte[] Data;
        }

        public class StringMarker
        {
            public int Offset;
        }

        public class CompressionEntry
        {
            public HashSet<StringEntry> Strings = new();
            public byte[] Data;
            public int Checksum;
            public int Impact { get => (Data.Length - 2) * Strings.Count; }
        }

        public static readonly byte[] _endChars = [0xC0, 0xCA, 0xD1];

        public static void Repack(string baseDir, string dbFile, Func<ChunkFile, DbRoot, IDictionary<string, Location>, uint> onProcess, Action<uint, object> onTransform)
        {
            var root = DbRoot.FromFile(dbFile);
            //var files = root.Files;

            //var gaps = DiscoverAvailableSpace(root, out var files);
            //var sfxFiles = DiscoverSfx(baseDir, root);
            var chunkFiles = DiscoverFiles(baseDir, root);
            //var patches = DiscoverPatches(baseDir, root, gaps);
            var patches = chunkFiles.Where(x => x.Type == BinType.Patch).ToList();

            var stdPatches = patches.Where(x => x.Bank != null).ToList();
            var nullPatches = patches.Where(x => x.Bank == null).ToList();

            //Process sfx files the same as others
            var allFiles = chunkFiles;// sfxFiles.Concat(chunkFiles).Concat(stdPatches).ToArray();
            var asmFiles = chunkFiles.Where(x => x.Blocks != null).ToArray();

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

            var sceneMeta = allFiles.Single(x => x.Name == "scene_meta" && x.Type == BinType.Assembly);

            var metaList = sceneMeta.Blocks[1].ObjList.OfType<string>()
                .Select(x => x.TrimStart(_addressspace).ToUpper()).Distinct().ToList();

            foreach (var b in allFiles.Where(x => x.Type == BinType.Bitmap && !metaList.Contains(x.Name.ToUpper())))
                b.Compressed = null;

            foreach (var b in allFiles.Where(x => x.Blocks == null
                && root.Files.Any(y => y.Type == x.Type && y.Name == x.Name && y.Upper == true)))
                b.Upper = true;

            //foreach (var str in sceneMeta.Blocks[1].ObjList.OfType<string>())
            //{
            //    var cf = chunkFiles.SingleOrDefault(x => x.Name.Equals(str.TrimStart(_addressspace)));
            //    if (cf != null && cf.Type == BinType.Bitmap)
            //        cf.Compressed = false;
            //}

            foreach (var f in allFiles.Where(x => x.Compressed != null))
                f.Size += 2;

            //RebuildDictionary(asmFiles);

            foreach (var asm in asmFiles)
                asm.CalculateSize();

            //Assign locations
            MatchChunks(allFiles);

            var blockLookup = new Dictionary<string, Location>();

            //Rebase assemblies
            //var hasBlocks = allFiles.Where(x => x.Blocks != null).ToArray();
            //foreach (var file in allFiles)
            //    file.Rebase();

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

            foreach (var file in asmFiles)
                file.Rebase();

            //Add other files to lookup
            //foreach (var f in root.Files.Except(allFiles.Select(x => x.File)))
            //    blockLookup[f.Name.ToUpper()] = f.Start;

            //Add files to lookup
            foreach (var f in allFiles)//.Except(patches))
                blockLookup[f.Name.ToUpper()] = f.Location;

            //Process includes
            foreach (var f in asmFiles.Where(x => x.Includes?.Any() == true))
            {
                f.IncludeLookup = asmFiles.Where(x => f.Includes.Contains(x.Name.ToUpper()))
                    .SelectMany(x => x.Blocks.Where(x => x.Label != null))
                    .ToDictionary(x => x.Label.ToUpper(), x => x.Location);
            }


            ////Process transforms
            //foreach (var tr in root.Transforms.Where(x => x.Value != ""))
            //{
            //    var name = tr.Value;
            //    char cmd = '&', op = (char)0;
            //    uint offset = 0;

            //    if (_addressspace.Contains(name[0]))
            //    {
            //        cmd = name[0];
            //        name = name[1..];
            //    }

            //    var ix = name.IndexOfAny(_operators);
            //    if (ix >= 0)
            //    {
            //        op = name[ix];
            //        offset = uint.Parse(name[(ix + 1)..], NumberStyles.HexNumber);
            //        name = name[..ix];
            //    }

            //    if (!blockLookup.TryGetValue(name.ToUpper(), out var loc))
            //    {
            //        var matchingBlock = asmFiles.SelectMany(x => x.Blocks)
            //            .SingleOrDefault(x => name.Equals(x.Label, StringComparison.CurrentCultureIgnoreCase));
            //        if (matchingBlock == null)
            //            continue;
            //        loc = matchingBlock.Location;
            //    }

            //    switch (op)
            //    {
            //        case '-': loc -= offset; break;
            //        case '+': loc += offset; break;
            //    }


            //    onTransform(tr.Key.Offset, cmd switch
            //    {
            //        '%' => (loc.Offset | 0x800000),
            //        '*' => (byte)(loc.Bank | 0xC0),
            //        '^' => (byte)(loc.Bank | 0x80),
            //        '&' => (object)(ushort)loc.Offset,
            //        _ => throw new($"Unsupported transform type '{cmd}'")
            //    });
            //}

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

            foreach (var file in nullPatches)
                onProcess(file, root, blockLookup);

            //Write entry points
            foreach (var e in root.EntryPoints)
            {
                foreach (var f in asmFiles)
                    if (f.Bank == 0)
                        foreach (var b in f.Blocks)
                            if (b.Label == e.Value)
                            {
                                onTransform(e.Key.Offset, (ushort)b.Location.Offset);
                                goto Next;
                            }
                        Next:;
            }

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

        private static void RebuildDictionary(IEnumerable<ChunkFile> asmFiles)
        {

            var stringEntries = new List<StringEntry>();

            var dictList = _dictionaries.Select(x => asmFiles.SingleOrDefault(y => y.Name == x)).ToArray();

            foreach (var file in dictList)
            //void fillDictionary(ChunkFile file)
            {
                var listBlock = file.Blocks[1];
                while (listBlock.ObjList.Count < 0x100)
                {
                    string label = $"{listBlock.Label}_entry_{listBlock.ObjList.Count:X2}";
                    listBlock.ObjList.Add($"&{label}");
                    listBlock.Size += 2;

                    var newBlock = new AsmBlock { Label = label, Size = 1 };
                    newBlock.ObjList.Add(new StringEntry { Data = [0xCA], Size = 1, Block = newBlock });
                    file.Blocks.Add(newBlock);
                }
            }

            //fillDictionary(dictionary1);
            //fillDictionary(dictionary2);
            //fillDictionary(dictionary3);

            var lookupList = dictList.Select(dict => dict.Blocks[1].ObjList
                .Select(x => (StringEntry)dict.Blocks.First(y => y.Label == ((string)x)[1..]).ObjList.First())
                .ToList()).ToList();


            //List<StringEntry> createLookup(ChunkFile dict) => dict.Blocks[1].ObjList
            //    .Select(x => (StringEntry)dict.Blocks.First(y => y.Label == ((string)x)[1..]).ObjList.First())
            //    .ToList();

            //var lookupList = new[] { createLookup, stringLookup2, stringLookup3 };

            var stringMatches = new List<CompressionEntry>();


            CompressionEntry addMatch(byte[] data, int index, int len)
            {
                //int checksum = data.Sum(x => x);

                foreach (var m in stringMatches)
                    if (m.Data.Length == len)// && checksum == m.Checksum)
                    {
                        int ix = 0;
                        while (ix < len)
                            if (m.Data[ix] != data[index + ix]) break;
                            else ix++;

                        if (ix == len)
                            return m;
                    }

                var newSample = new byte[len];
                Array.Copy(data, index, newSample, 0, len);

                var newMatch = new CompressionEntry { Data = newSample };//, Checksum = checksum };
                stringMatches.Add(newMatch);
                return newMatch;
            }

            //Expand strings
            foreach (var asm in asmFiles.Except(dictList))
                foreach (var block in asm.Blocks)
                    foreach (var part in block.ObjList)
                        if (part is StringEntry se)
                        {
                            var data = se.Data;
                            for (int i = 0; i >= 0 && i < data.Length;)
                            {
                                var c = data[i];

                                var lookupIx = Array.IndexOf(_dictCommands, c);

                                if (lookupIx < 0)
                                {
                                    i = getNext(data, i);
                                    continue;
                                }

                                var lookup = lookupList[lookupIx];

                                var ix = data[i + 1];
                                var str = lookup[ix];
                                int len = str.Data.Length - 1;
                                int newSize = data.Length + len - 2;
                                byte[] newData = new byte[newSize];
                                Array.Copy(data, newData, i);
                                Array.Copy(str.Data, 0, newData, i, len);
                                Array.Copy(data, i + 2, newData, i + len, data.Length - (i + 2));

                                se.Block.Size += len - 2;
                                se.Size += len - 2;
                                se.Data = data = newData;

                                //if (se.Block.Size != newData.Length)
                                //{

                                //}

                                i += len;
                            }
                            stringEntries.Add(se);
                        }


            static int getNext(byte[] buffer, int index)
            {
                if (index >= buffer.Length)
                    return -1;

                return buffer[index] switch
                {
                    0xC0 or 0xCA or 0xD1 => -1,
                    0xC5 or 0xC6 => index + 5,
                    0xCD or 0xD4 => index + 4,
                    0xC1 or 0xC7 => index + 3,
                    0xC2 or 0xC3 or 0xC9 or 0xCC or 0xD2 or 0xD5 or 0xD6 or 0xD7 => index + 2,
                    0xD8 => advanceEscape(),
                    _ => index + 1
                };
                int advanceEscape() { while (buffer[++index] != 0x00) ; return index + 1; }
            }

            var stringCount = stringEntries.Count;
            var minMatchLength = 5;

            void walkEntry(int ix)
            {
                var se = stringEntries[ix];
                var srcData = se.Data;
                var srcLen = srcData.Length;
                while (++ix < stringCount)
                {
                    var other = stringEntries[ix];
                    var dstData = other.Data;
                    var dstLen = dstData.Length;
                    int eix = 0;
                    for (int six = 0; six >= 0 && six < srcLen; six = getNext(srcData, six))
                    {
                        //Minimum of 3 bytes
                        eix = getNext(srcData, six);
                        while (eix < srcData.Length && eix >= 0 && eix - six < minMatchLength)// && !_endChars.Contains(srcData[eix]))
                            eix = getNext(srcData, eix);

                        if (eix < 0 || eix - six < minMatchLength)
                            break;

                        for (int dix = 0; dix >= 0 && dix < dstLen; dix = getNext(dstData, dix))
                        {
                            int fix = six;
                            int nix = dix;

                            bool compare()
                            {
                                if (fix >= srcLen || nix >= dstLen)
                                    return false;

                                var c = srcData[fix];
                                if (c == 0xC0 || c == 0xCA || c == 0xD1)
                                    //if (_endChars.Contains(srcData[fix]))
                                    return false;

                                while (fix < eix)
                                {
                                    if (fix >= srcLen || nix >= dstLen || srcData[fix] != dstData[nix])
                                        return false;
                                    fix++;
                                    nix++;

                                }
                                return true;
                            }

                            int bestEnd = -1;
                            while (compare())
                                eix = getNext(srcData, bestEnd = eix);

                            if (bestEnd - six >= minMatchLength)
                            {
                                var entry = addMatch(srcData, six, bestEnd - six);
                                entry.Strings.Add(se);
                                entry.Strings.Add(other);
                                six = bestEnd;

                                //Minimum of 3 bytes
                                eix = getNext(srcData, six);
                                while (eix < srcData.Length && eix >= 0 && eix - six < minMatchLength)// && !_endChars.Contains(srcData[eix]))
                                    eix = getNext(srcData, eix);

                                if (eix < 0 || eix - six < minMatchLength)
                                    break;
                            }

                        }
                    }
                }
            }

            for (int i = 0; i < stringCount; i++)
                walkEntry(i);

            ////Combine similar matches
            //for (int i = 0; i < stringCount; i++)
            //{
            //    Top:
            //    var match = stringMatches[i];
            //    var mData = match.Data;
            //    for (int x = i + 1; x < stringCount;)
            //    {
            //        var other = stringMatches[x];
            //        var oData = other.Data;

            //        int z = 1;

            //        if (mData[0] == oData[0])
            //        {
            //            while (z < mData.Length && z < oData.Length)
            //            {
            //                if (mData[z] != oData[z])
            //                    break;
            //                z++;
            //            }
            //        }
            //        else if (mData[^1] == oData[^1])
            //        {
            //            z++;
            //            while (z <= mData.Length && z <= oData.Length)
            //            {
            //                if (mData[^z] != oData[^z])
            //                    break;
            //                z++;
            //            }
            //            z--;
            //        }

            //        if (z >= 3)
            //        {
            //            var (six, dix, src, dst) = mData.Length >= oData.Length
            //                ? (x, i, match, other)
            //                : (i, x, other, match);

            //            var newEntry = new CompressionEntry() { Data = dst.Data, Strings = new(dst.Strings) };
            //            foreach (var e in src.Strings)
            //                newEntry.Strings.Add(e);

            //            if (newEntry.Impact > src.Impact + dst.Impact)
            //            {
            //                stringMatches[i] = newEntry;
            //                stringMatches.RemoveAt(x);
            //                stringCount--;
            //                goto Top;
            //            }
            //        }

            //        x++;
            //    }
            //}

            var dictionary = stringMatches.OrderByDescending(x => x.Impact)
                .Take(_dictionaries.Length << 8).ToArray();

            int matchIx = 0;
            foreach (var match in dictionary)
            {
                var data = match.Data;
                var lookupIx = matchIx >> 8;
                var oldEntry = lookupList[lookupIx][(byte)matchIx];

                var newData = new byte[data.Length + 1];
                Array.Copy(data, newData, data.Length);
                newData[^1] = 0xCA;
                oldEntry.Data = newData;
                oldEntry.Block.Size += newData.Length - oldEntry.Size;
                oldEntry.Size = newData.Length;

                foreach (var str in match.Strings)
                {
                    var strData = str.Data;
                    for (int ix = 0; ix >= 0 && ix < strData.Length;)
                    {
                        int mix = 0, six = ix;
                        while (mix < data.Length && six < strData.Length)
                            if (data[mix] != strData[six++]) break;
                            else mix++;

                        if (mix == data.Length)
                        {
                            var moreData = new byte[strData.Length - mix + 2];
                            Array.Copy(strData, moreData, ix);
                            moreData[ix] = _dictCommands[lookupIx];
                            moreData[ix + 1] = (byte)matchIx;
                            Array.Copy(strData, ix + mix, moreData, ix + 2, strData.Length - (ix + mix));

                            str.Data = moreData;
                            str.Block.Size -= mix - 2;
                            str.Size = moreData.Length;

                            strData = moreData;
                            ix += 2;
                        }
                        else
                            ix = getNext(strData, ix);
                    }
                }

                matchIx++;
            }


        }

        //private static List<DbGap> DiscoverAvailableSpace(DbRoot root, out IEnumerable<DbFile> files)
        //{
        //    var sfxStart = root.Sfx.Location.Offset;
        //    List<DbGap> gaps = new([new DbGap { Start = 0x200000u, End = Location.MaxValue }]);
        //    files = root.Files.ToList();
        //    //var sfxNearest = Location.MaxValue;

        //    void mergeGap(Location start, Location end)
        //    {
        //        //if (sfxStart < end && sfxEnd > start)
        //        //{
        //        //    if (start >= sfxStart && (start.Offset & 0x8000u) == 0)
        //        //        start = sfxEnd;
        //        //    else if (((end.Offset - 1) & 0x8000u) == 0)
        //        //        end = sfxStart;
        //        //}

        //        //if (start >= sfxEnd && start < sfxNearest)
        //        //    sfxNearest = start;

        //        if (end <= start)
        //            return;

        //        for (int i = 0; i < gaps.Count; i++)
        //        {
        //            var g = gaps[i];
        //            Location s = g.Start, e = g.End;
        //            if (s <= end && e >= start)
        //            {
        //                gaps.RemoveAt(i);
        //                mergeGap(start < s ? start : s, end > e ? end : e);
        //                return;
        //            }
        //        }

        //        gaps.Add(new DbGap { Start = start, End = end });
        //    }

        //    //Merge SFX ranges
        //    var sfxLoc = root.Sfx.Location.Offset;
        //    var sfxSize = root.Sfx.Size.Value;
        //    while (sfxSize > 0)
        //    {
        //        var s = Math.Min(sfxSize, 0x8000u);
        //        mergeGap(sfxLoc, sfxLoc + s);
        //        sfxLoc += 0x10000;
        //        sfxSize -= s;
        //    }

        //    //Merge the file ranges
        //    foreach (var file in files)
        //        mergeGap(file.Start, file.End);

        //    //Gerge gap ranges
        //    foreach (var space in root.FreeSpace)
        //        mergeGap(space.Start, space.End);

        //    //while (sfxEnd < sfxNearest)
        //    //{
        //    //    var gapStart = sfxNearest & 0x3F0000u;

        //    //    if (gapStart <= sfxEnd)
        //    //    {
        //    //        mergeGap(sfxEnd, sfxNearest);
        //    //        break;
        //    //    }
        //    //    else
        //    //    {
        //    //        mergeGap(gapStart, sfxNearest);
        //    //        sfxNearest = gapStart - 0x8000u;
        //    //    }
        //    //}

        //    //Split gaps across chunk boundaries
        //    for (int i = 0; i < gaps.Count; i++)
        //    {
        //        var g = gaps[i];
        //        Location start = g.Start, end = g.End;
        //        var next = start + (ChunkSize - (start.Offset % ChunkSize));
        //        if (next == 0u) next--;
        //        if (end > next)
        //        {
        //            g.End = next;
        //            gaps.Add(new DbGap { Start = next, End = end });
        //        }
        //    }

        //    //How much space do we have?
        //    //var newSize = gaps.Sum(x => x.End.Offset - x.Start.Offset);

        //    return gaps;
        //}

        private static List<ChunkFile> DiscoverSfx(string baseDir, DbRoot root)
        {
            var res = root.GetPath(BinType.Sound);
            return root.Sfx.Names.Select(name =>
            {
                var path = Path.Combine(baseDir, res.Folder ?? "", $"{name}.{res.Extension}");
                return new ChunkFile(path, BinType.Sound)
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


        private static List<ChunkFile> DiscoverFiles(string baseDir, DbRoot root)
        {
            var chunkFiles = new List<ChunkFile>();
            foreach (BinType type in typeof(BinType).GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.GetValue(null)))
            {
                if (type == BinType.Transform || type == BinType.Meta17)
                    continue;

                var res = root.GetPath(type);

                foreach (var file in Directory.GetFiles(Path.Combine(baseDir, res.Folder), $"*.{res.Extension}", SearchOption.AllDirectories))
                {
                    var chunkFile = new ChunkFile(file, type);

                    if (type == BinType.Unknown && chunkFile.Name.StartsWith("meta17"))
                    {
                        chunkFile.Type = BinType.Meta17;
                        chunkFile.Compressed = false;
                    }

                    if (type == BinType.Assembly || type == BinType.Patch)
                        using (var inFile = File.OpenRead(file))
                            (chunkFile.Blocks, chunkFile.Includes, chunkFile.Bank) = ParseAssembly(root, inFile, 0);
                    else if (type == BinType.Sound)
                        chunkFile.Size += 2;

                    chunkFiles.Add(chunkFile);
                }

            }

            return chunkFiles;
        }


        private static List<ChunkFile> DiscoverFiles(string baseDir, DbRoot root, IEnumerable<DbFile> files)
            => files.Select(file =>
            {
                var res = root.GetPath(file.Type);
                //string folder = "misc", extension = "bin";
                //switch (file.Type)
                //{
                //    case BinType.Bitmap: folder = "graphics"; break;
                //    case BinType.Palette: folder = "palettes"; extension = "pal"; break;
                //    case BinType.Music: folder = "music"; extension = "bgm"; break;
                //    case BinType.Tileset: folder = "tilesets"; extension = "set"; break;
                //    case BinType.Tilemap: folder = "tilemaps"; extension = "map"; break;
                //    case BinType.Meta17: break;
                //    case BinType.Spritemap: folder = "spritemaps"; break;
                //    case BinType.Assembly: folder = "asm"; extension = "asm"; break;
                //        //case BinType.Sound: folder = "sounds"; extension = "sfx"; break;
                //};

                var path = Path.Combine(baseDir, res.Folder ?? "", $"{file.Name}.{res.Extension}");// File.Exists(path) ? path : Path.Combine(baseDir, path);
                int size;
                List<AsmBlock>? blocks = null;
                HashSet<string>? includes = null;
                byte? bank = null;
                if (file.Type == BinType.Assembly)
                {
                    using (var inFile = File.OpenRead(path))
                        (blocks, includes, bank) = ParseAssembly(root, inFile, file.Start);

                    size = 0;// ChunkFile.CalculateSize(blocks);
                }
                else
                {
                    size = (int)new FileInfo(path).Length;

                    if (file.Compressed != null)
                        size += 2;
                }
                return new ChunkFile(path, file.Type) { Size = size, Blocks = blocks, Includes = includes, Bank = bank };
            }).ToList();

        //private static List<ChunkFile> DiscoverPatches(string baseDir, DbRoot root)
        //{
        //    var res = root.GetPath(BinType.Patch);
        //    var patchList = new List<ChunkFile>();

        //    foreach (var file in Directory.GetFiles(Path.Combine(baseDir, res.Folder ?? ""), $"*.{res.Extension}"))
        //    {
        //        var chunkFile = new ChunkFile(file, BinType.Assembly);

        //        using (var fileStream = File.OpenRead(file))
        //            (chunkFile.Blocks, chunkFile.Includes, chunkFile.Bank) = ParseAssembly(root, fileStream, 0);

        //        patchList.Add(chunkFile);
        //    }

        //    return patchList;
        //}

        private static void MatchChunks(IEnumerable<ChunkFile> files)
        {
            var allFiles = files.Where(x => x.Size > 0).OrderByDescending(x => x.Size).ToList();
            var bestResult = new int[0x100];
            var bestSample = new int[0x100];

            for (int page = 0; page < 0x80; page++)
            {
                var remain = 0x8000;

                //Account for SNES header
                if (page == 1)
                    remain -= 0x50;

                //Stop when there are no more files
                var count = allFiles.Count;
                if (count == 0)
                    break;

                var isUpper = (page & 1) != 0;
                var bank = page >> 1;
                var smallest = allFiles.LastOrDefault(x => (isUpper && x.Bank == bank) 
                    || (!isUpper && x.Blocks == null));
                var smallestIx = smallest == null ? -1 : allFiles.IndexOf(smallest);
                int bestDepth = 0, bestRemain = remain, bestOffset = 0;
                int start = page << 15;
                //}

                //foreach (var gap in gaps.OrderBy(x => x.Size).ThenBy(x => x.Start.Offset))
                //{
                //    var remain = gap.Size;
                //    var count = allFiles.Count;
                //    if (count == 0)
                //        break;
                //    var smallest = allFiles[^1].Size;
                //    var isUpper = (gap.Start.Offset & 0x8000) != 0;
                //    var bank = gap.Start.Bank;
                //    int bestDepth = 0, bestRemain = remain, bestOffset = 0;

                bool testDepth(int ix, int depth, int remain, bool asmMode)
                {
                    if (ix > smallestIx)
                        return true;

                    for (var i = ix; i < count; i++)
                    {
                        var file = allFiles[i];
                        if (file.Size > remain)
                            continue;

                        if (file.Blocks != null)
                        {
                            if (!asmMode)
                            {
                                if (!isUpper || file.Bank != null)
                                    continue;
                            }
                            else if (file.Bank != bank)// || file.File.Move == true)
                                continue;

                        }
                        else if (asmMode)
                            continue;
                        else if (file.Upper && !isUpper)
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
                        if (newRemain < 0x20)
                            return true;

                        //Stop processing if nothing else can fit
                        if (newRemain < smallest?.Size)
                            return false;

                        //Process next iteration and stop if exact
                        if (testDepth(i + 1, depth + 1, newRemain, asmMode))
                            return true;
                    }

                    return true;
                };

                testDepth(0, 0, remain, isUpper);
                if (isUpper && bestRemain >= (smallest?.Size ?? 0))
                {
                    bestOffset = bestDepth;
                    smallest = allFiles.LastOrDefault(x => x.Blocks == null || x.Bank == null);
                    smallestIx = smallest == null ? -1 : allFiles.IndexOf(smallest);
                    testDepth(0, bestDepth, bestRemain, false);
                }

                var position = start;
                for (int i = 0; i < bestDepth;)
                {
                    var file = allFiles[bestResult[i++]];
                    file.Location = (uint)position;
                    //onProcess?.Invoke(file);
                    Console.WriteLine($"  {position:X6}: {file.Name}");
                    position += file.Size;
                }

                Console.WriteLine($"Page {start:X6} matched with {bestDepth} files {bestRemain:X} remaining");

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
                throw new($"Unable to match {allFiles.Count} files, perhaps there is no room\r\n! {string.Join("\r\n", allFiles.Select(x => x.Name))}");

        }


        public static int GetSize(object obj)
        {
            //var obj = objList[index];
            if (obj is Op op)
                return op.Size;
            else if (obj is byte[] arr)
                return arr.Length;
            else if (obj is StringEntry se)
                return se.Data.Length;
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
                        case '*': return 2;
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

        public static char[]
            _whitespace = [' ', '\t'],
            _operators = ['-', '+'],
            _commaspace = [',', ' ', '\t'],
            _addressspace = ['@', '&', '^', '#', '$', '%', '*'],
            _symbolSpace = [',', ' ', '\t', '<', '>', '(', ')', ':', '[', ']', '{', '}', '`', '~', '|'],
            _labelSpace = ['[', '{', '#', '`', '~', '|', ':'],
            _objectspace = ['<', '['],
            _copSplitChars = [' ', '\t', ',', '(', ')', '[', ']', '$', '#'],
            _stringSpace = ['~', '`', '|'];

        private class StringSizeComparer : IComparer<string>
        {
            public int Compare(string? x, string? y)
            {
                if (x == null)
                    return (y == null) ? 0 : 1;
                else if (y == null)
                    return -1;

                return (x.Length > y.Length) ? -1 : (x.Length < y.Length) ? 1 : string.Compare(x, y);
            }
        }

        private static StringSizeComparer _staticComparer = new StringSizeComparer();

        public static (List<AsmBlock>, HashSet<string>, byte?) ParseAssembly(DbRoot root, Stream inStream, uint startLoc)//, out IDictionary<string, Location> includes) //, IDictionary<string, Location> chunkLookup)
        {
            using var reader = new StreamReader(inStream);

            var includes = new HashSet<string>();
            var blocks = new List<AsmBlock>();
            AsmBlock current;
            //AsmBlock? target;
            var tags = new SortedDictionary<string, string?>(_staticComparer);
            var memStream = new MemoryStream();
            int ix, bix = 0, lineCount = 0;
            byte? bank = null;
            //string? lastStruct = null;
            HexString? lastDelimiter = null;

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
                {
                    var cix = line.IndexOfAny(_stringSpace);
                    if (cix < 0 || cix > ix || (cix = line.LastIndexOfAny(_stringSpace)) < ix)
                        line = line[..ix];
                }

                //Ignore comments
                if ((ix = line.IndexOf(';')) >= 0)
                {
                    var cix = line.IndexOfAny(_stringSpace);
                    if (cix < 0 || cix > ix || (cix = line.LastIndexOfAny(_stringSpace)) < ix)
                        line = line[..ix];
                }

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
                                includes.Add(value.ToUpper().Replace("'", ""));
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

            void processText(string? struc = null, char? openTag = null, bool saveDelimiter = false)
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

                if (saveDelimiter)
                    lastDelimiter = dbs?.Delimiter ?? parent?.Delimiter;

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


                    string? mnem = null, operand = null, operand2 = null;
                    while (line.Length > 0)
                    {
                        if (line[0] == ';')
                        {
                            line = "";
                            break;
                        }

                        //Process strings
                        if (_stringSpace.Contains(line[0]))
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

                            byte? lastCmd = null;
                            Rom.StringType stringType;

                            switch (c)
                            {
                                case '`':
                                    stringType = Rom.StringType.Wide;
                                    ProcessString(root.WideCommands, root.WideMap, i => (byte)((i & 0x70) << 1 | (i & 0x0F)));
                                    goto Terminate;

                                case '~':
                                    stringType = Rom.StringType.Char;
                                    ProcessString(root.WideCommands, root.CharMap, i => (byte)((i & 0x38) << 1 | (i & 0x07)));
                                Terminate:
                                    if (lastCmd == null || !_endChars.Contains(lastCmd.Value))
                                        memStream.WriteByte(0xCA);
                                    break;

                                case '|':
                                    stringType = Rom.StringType.ASCII;
                                    ProcessString(root.StringCommands, null, null);
                                    memStream.WriteByte(0);
                                    break;

                                default: throw new("Unsupported string type");
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

                                        x = ix;

                                        //Marker
                                        if (parts.Length == 0)
                                        {
                                            flushBuffer(true);
                                            current.ObjList.Add(new StringMarker { Offset = current.Size });
                                            continue;
                                        }

                                        var cmd = dict.Values.SingleOrDefault(x => x.Value == parts[0]);
                                        if (cmd != null)
                                        {
                                            lastCmd = (byte)cmd.Code.Value;
                                            memStream.WriteByte(lastCmd.Value);

                                            if (cmd.Types != null)
                                            {
                                                var hasPointer = cmd.Types.Contains(MemberType.Address) || cmd.Types.Contains(MemberType.Offset);
                                                if (hasPointer)
                                                    flushBuffer(true);

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
                                                            flushBuffer(false);
                                                            current.ObjList.Add(parts[pix]);
                                                            current.Size += cmd.Types[y] == MemberType.Offset ? 2 : 3;
                                                            break;
                                                    }
                                                }

                                                if (hasPointer)
                                                    flushBuffer(false);
                                            }
                                            else
                                            {

                                            }


                                            continue;
                                        }
                                    }

                                    lastCmd = null;
                                    processChar(c);
                                }

                            }

                            void flushBuffer(bool wrap = false)
                            {
                                var buffer = memStream.GetBuffer();
                                var size = (int)memStream.Length;
                                if (size > 0)
                                {
                                    var newBuffer = new byte[size];
                                    Array.Copy(buffer, newBuffer, size);
                                    current.ObjList.Add(wrap && stringType == StringType.Wide
                                        ? new StringEntry { Data = newBuffer, Block = current, Index = current.ObjList.Count, Size = newBuffer.Length }
                                        : newBuffer);
                                    current.Size += size;
                                    memStream.Position = 0;
                                    memStream.SetLength(0);
                                }
                            }

                            flushBuffer(true);
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
                                    if (data.Length == 1)
                                    {
                                        current.ObjList.Add(data[0]);
                                        current.Size++;
                                    }
                                    else
                                    {
                                        if (reverse)
                                            for (int x = 0, y = data.Length; --y > x;)
                                            {
                                                var sample = data[x];
                                                data[x++] = data[y];
                                                data[y] = sample;
                                            }

                                        if (data.Length == 2)
                                        {
                                            current.ObjList.Add((ushort)(data[0] | data[1] << 8));
                                            current.Size += 2;
                                        }
                                        else
                                        {
                                            current.ObjList.Add(data);
                                            current.Size += data.Length;
                                        }
                                    }
                                }
                            }

                            if (openTag == '[' && lastDelimiter != null)
                                lastDelimiter = null;

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

                            if (delimiter == null && lastDelimiter != null)
                                delimiter = lastDelimiter;
                            //delimiter = root.Structs.Values.Single(x => x.Name.Equals(lastStruct, StringComparison.CurrentCultureIgnoreCase)).Delimiter;

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
                                processText(mnem, '<', openTag == '[' && currentType == null);
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

                                //if (mnem.Length == 6
                                //    && uint.TryParse(mnem, NumberStyles.HexNumber, null, out var addr)
                                //    && Location.MaxValue >= addr)
                                //{
                                //    blocks.Add(current = new() { Location = addr });
                                //    bix++;
                                //}
                                //else if (mnem.Length > 0)
                                //{
                                blocks.Add(current = new()
                                {
                                    Location = current.Location + (uint)current.Size,
                                    Label = mnem,
                                    IsString = _stringSpace.Contains(operand[0])
                                });
                                bix++;
                                //}

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
                        if ((ix = operand.IndexOfAny(_operators)) >= 0)
                        {
                            var op = operand[ix];

                            int vix = operand.LastIndexOf('$', ix) + 1;
                            //if (vix == 0)
                            //    throw new($"Unable to locate variable for operator line {lineCount}: '{op}'");

                            if (uint.TryParse(operand[vix..ix], NumberStyles.HexNumber, null, out var value))
                            {
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
                                    opCode = code;
                                    operand = match.Groups[1].Value;
                                    if (match.Groups.Count > 2)
                                        operand2 = match.Groups[2].Value;
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
                                if (eix >= 0)
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

                        object parseOpnd(string opnd) => opnd.Length switch
                        {
                            2 when byte.TryParse(opnd, NumberStyles.HexNumber, null, out var b) => b,
                            4 when ushort.TryParse(opnd, NumberStyles.HexNumber, null, out var s) => s,
                            6 when uint.TryParse(opnd, NumberStyles.HexNumber, null, out var i) => i,
                            _ => opnd
                        };

                        object opnd1 = parseOpnd(operand);

                        int size = opCode.Size;
                        if (size == -2)
                            size = (operand[0] == '^' || opnd1 is byte) ? 2 : 3;

                        object[] opnds = (operand2 != null)
                            ? [opnd1, parseOpnd(operand2)]
                            : [opnd1];

                        current.ObjList.Add(new Op() { Code = opCode, Operands = opnds, Size = (byte)size });
                        current.Size += size;
                    //}
                    Next:;
                    }
                }
            }

            processText(null);

            return (blocks, includes, bank);
        }

    }
}
