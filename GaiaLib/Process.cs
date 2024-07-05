using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Structs;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

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
        }

        public static void Repack(string baseDir, string dbFile, Func<ChunkFile, DbRoot, IDictionary<string, Location>, uint> onProcess)
        {
            var root = DbRoot.FromFile(dbFile);

            var gaps = DiscoverAvailableSpace(root, out var files);
            var sfxFiles = DiscoverSfx(baseDir, root);
            var chunkFiles = DiscoverFiles(baseDir, root, files);
            var patches = DiscoverPatches(baseDir, gaps);

            //Process sfx files the same as others
            var allFiles = sfxFiles.Concat(chunkFiles).OrderBy(x => x.Location);

            //Assign locations
            MatchChunks(gaps, allFiles);

            //Generate lookup table
            //var chunkLookup = allFiles.ToDictionary(x => x.File.Name.ToUpper(), x => x.Location);

            var chunkLookup = allFiles
                .Concat(root.Files.Except(allFiles.Select(x => x.File))
                .Select(x => new ChunkFile { File = x, Location = x.Start }))
                .ToDictionary(x => x.File.Name.ToUpper(), x => x.Location);

            //Write file contents
            foreach (var file in allFiles)
                onProcess(file, root, chunkLookup);

            //Write patch contents
            foreach (var grp in patches.GroupBy(x => x.Location))
            {
                var loc = grp.Key.Offset;
                foreach (var file in grp)
                {
                    if (loc > 0) file.Location = loc;
                    var size = onProcess(file, root, chunkLookup);
                    if (loc > 0) loc += size;
                }
            }
        }

        private static List<DbGap> DiscoverAvailableSpace(DbRoot root, out IEnumerable<DbFile> files)
        {
            var sfxStart = root.Sfx.Location.Offset;
            List<DbGap> gaps = new([new DbGap { Start = 0x200000u, End = Location.MaxValue }]);
            files = root.Files.Where(x => x.XRef != null || x.HRef != null || x.LRef != null).ToList();
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
                return new ChunkFile
                {
                    Path = path,
                    Size = (int)(new FileInfo(path).Length + 2),
                    File = new()
                    {
                        Type = BinType.Sound,
                        Name = name
                    }
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
                int size = 0;
                List<AsmBlock>? blocks = null;
                if (file.Type == BinType.Assembly)
                {
                    using (var inFile = File.OpenRead(path))
                        blocks = ParseAssembly(root, inFile, file.Start);

                    size = blocks.Sum(x => x.Size);
                }
                else
                {
                    size = (int)new FileInfo(path).Length;

                    if (file.Compressed != null)
                        size += 2;
                }
                return new ChunkFile { File = file, Path = path, Size = size, Blocks = blocks };
            }).ToList();

        private static List<ChunkFile> DiscoverPatches(string baseDir, List<DbGap> gaps)
        {
            string folder = "patches", extension = "asm";
            var patchList = new List<ChunkFile>();

            var patchReserves = new Dictionary<byte, uint>();

            uint getOrReserve(byte bank)
            {
                bank &= 0x3F;
                if (patchReserves.TryGetValue(bank, out uint location))
                    return location;

                location = ((uint)bank << 16) | 0x8000u;
                var gap = gaps.Where(x => x.Start.Bank == bank && x.Start >= location)
                    .OrderByDescending(x => x.Size)
                    .FirstOrDefault()
                    ?? throw new($"Unable to reserve patch space for bank {bank:X2}");

                Console.WriteLine($"Reserving space {gap.Start} - {gap.End} for patches");

                gaps.Remove(gap);
                return patchReserves[bank] = gap.Start;
            }

            foreach (var file in Directory.GetFiles(Path.Combine(baseDir, folder), $"*.{extension}"))
            {
                byte? bank = null;

                //Discover bank
                using (var fileStream = File.OpenRead(file))
                using (var reader = new StreamReader(fileStream))
                {
                    var line = (reader.ReadLine() ?? "").Trim().ToUpper();
                    if (line.StartsWith("?BANK"))
                        bank = byte.Parse(line[5..].Trim(), NumberStyles.HexNumber);
                    //else
                    //    continue;
                }

                patchList.Add(new ChunkFile
                {
                    Path = file,
                    Location = bank != null ? getOrReserve(bank.Value) : 0u,
                    File = new() { Type = BinType.Assembly }
                });
            }

            return patchList;
        }

        private static void MatchChunks(IEnumerable<DbGap> gaps, IEnumerable<ChunkFile> files)
        {
            var allFiles = files.OrderByDescending(x => x.Size).ToList();
            var bestBuffer = new int[0x100];
            var bestSample = new int[0x100];

            foreach (var gap in gaps.OrderBy(x => x.Size).ThenBy(x => x.Start.Offset))
            {
                var remain = gap.Size;
                var count = allFiles.Count;
                if (count == 0)
                    break;
                var smallest = allFiles[count - 1].Size;

                int bestDepth = 0, bestRemain = remain;

                bool testDepth(int ix, int depth, int remain)
                {
                    if (ix >= count)
                        return true;

                    for (var i = ix; i < count; i++)
                    {
                        var file = allFiles[i];
                        if (file.Size > remain)
                            continue;

                        bestSample[depth] = i;

                        var newRemain = remain - file.Size;
                        if (newRemain < bestRemain)
                        {
                            bestRemain = newRemain;
                            bestDepth = depth + 1;
                            Array.Copy(bestSample, bestBuffer, bestDepth);
                        }

                        //Stop when we have an "exact" match
                        if (newRemain < 40)
                            return true;

                        //Stop processing if nothing else can fit
                        if (newRemain < smallest)
                            return false;

                        //Process next iteration and stop if exact
                        if (testDepth(i + 1, depth + 1, newRemain))
                            return true;
                    }

                    return false;
                };

                testDepth(0, 0, remain);

                var position = gap.Start;
                for (int i = 0; i < bestDepth;)
                {
                    var file = allFiles[bestBuffer[i++]];
                    file.Location = position;
                    //onProcess?.Invoke(file);
                    position += (uint)file.Size;
                }

                Console.WriteLine($"Chunk {gap.Start} - {gap.End} matched with {bestDepth} files {bestRemain:X} remaining");

                for (int i = bestDepth; i > 0;)
                    allFiles.RemoveAt(bestBuffer[--i]);

            }

        }

        private static void ProcessPatches(IEnumerable<ChunkFile> patches, Func<ChunkFile, uint> process)
        {

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
                    if (str[0] == '@')
                        return 3;
                    else if (str[0] == '&')
                        return 2;
                    else if (str[0] == '^')
                        return 1;
            }
            ///TODO: Add parsers for strings
            throw new($"Unable to get size for operand '{obj}'");
        }

        private static char[]
            _whitespace = [' ', '\t'],
            _operators = ['-', '+'],
            _commaspace = [',', ' ', '\t'],
            _addressspace = ['@', '&', '^', '#', '$'],
            _objectspace = ['<', '['];

        public static List<AsmBlock> ParseAssembly(DbRoot root, Stream inStream, uint startLoc) //, IDictionary<string, Location> chunkLookup)
        {
            using var reader = new StreamReader(inStream);

            var blocks = new List<AsmBlock>();
            AsmBlock current;
            AsmBlock? target;
            var tags = new Dictionary<string, string?>();
            int ix, bix = 0, lineCount = 0;

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

                //Make everything case-insensitive
                line = line.ToUpper();

                //Process directives
                if (line[0] == '?')
                {
                    ///TODO: Something later
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
                    while ((ix = line.IndexOf(tag.Key)) >= 0)
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
                        current.Size += GetSize(obj);
                        return true;
                    }
                    return false;
                }

                void AdvancePart()
                {
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
                        //Process strings
                        if (line[0] == '`')
                        {
                            string? str = null;
                            if ((ix = line.IndexOf('`', 1)) >= 0)
                            {
                                str = line[1..ix];
                                line = line[ix..];
                            }
                            else
                            {
                                str = line;
                                line = "";
                            }

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

                            AdvancePart();
                            continue;
                        }


                        if (line[0] == '>')
                        {
                            if (openTag == '<')
                                line = line[1..].TrimStart(_commaspace);
                            return;
                        }


                        if (line[0] == ']')
                        {
                            if (openTag == '[')
                                line = line[1..].TrimStart(_commaspace);
                            if (delimiter != null)
                            {
                                var obj = delimiter.Value.ToObject();
                                current.ObjList.Add(obj);
                                current.Size += GetSize(obj);
                            }
                            return;
                        }

                        if (line[0] == '[')
                        {
                            line = line[1..].TrimStart(_commaspace);
                            processText(currentType, '[');
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

                        //Process labels
                        if ((ix = line.IndexOf(':')) >= 0)
                        {
                            var label = line[..ix].TrimEnd(_commaspace);

                            if (label.StartsWith('$'))
                                label = label[1..];

                            if (label.Length == 6
                                && uint.TryParse(label, NumberStyles.HexNumber, null, out var addr)
                                && Location.MaxValue >= addr)
                            {
                                blocks.Add(current = new() { Location = addr });
                                bix++;
                            }
                            else if (label.Length > 0)
                            {
                                //if (current.Size == 0 && current.Label == null)
                                //    current.Label = label;
                                //else
                                //{
                                blocks.Add(current = new() { Location = current.Location + (uint)current.Size, Label = label });
                                bix++;
                                //}
                            }

                            line = line[(ix + 1)..].TrimStart(_commaspace);
                            continue;
                        }

                        //Separate instructions into mnemonic and operand parts
                        if ((ix = line.IndexOfAny(_commaspace)) > 0)
                        {
                            mnem = line[..ix];
                            operand = line[(ix + 1)..].TrimStart(_commaspace);

                            //Process object tags
                            if (operand.StartsWith('<'))
                            {
                                line = operand[1..].TrimStart(_commaspace);
                                processText(mnem, '<');
                                mnem = null;
                                continue;
                            }
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
                            mnem = line;

                        line = "";
                        break;
                    }

                    if (mnem?.Length > 0)
                    {
                        //Get list of opcodes from mnemonic
                        if (!OpCode.Grouped.TryGetValue(mnem, out var codes))
                        {
                            //No mnemonic? Make a label!
                            if (operand != null && operand[0] == '[')
                            {
                                blocks.Add(current = new() { Label = mnem, Location = current.Location + (uint)current.Size });
                                bix++;
                                line = operand[1..];
                                continue;
                            }
                            throw new($"Unknown instruction line {lineCount}: '{mnem}'");
                        }

                        //No operand instructions
                        if (string.IsNullOrEmpty(operand))
                        {
                            current.ObjList.Add(new Op { Code = codes.First(x => x.Size == 1), Operands = [], Size = 1 });
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

                            var endIx = operand.IndexOfAny(_commaspace, ix + 1);
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
                    }
                }
            }

            processText(null);

            return blocks;
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
