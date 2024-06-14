using GaiaLib.Database;
using GaiaLib.Structs;
using System.Text;

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
        }

        public static void Repack(string baseDir, string dbFile = "database.json", Action<ChunkFile> onProcess = null)
        {
            var root = DbRoot.FromFile(dbFile);

            var gaps = DiscoverAvailableSpace(root, out var files);
            var chunkFiles = DiscoverFiles(baseDir, files);

            var bestBuffer = new int[0x100];
            var bestSample = new int[0x100];

            foreach (var gap in gaps.OrderBy(x => x.Size))
            {
                var remain = gap.Size;
                var count = chunkFiles.Count;
                if (count == 0)
                    break;
                var smallest = chunkFiles[count - 1].Size;

                int bestDepth = 0, bestRemain = remain;

                bool testDepth(int ix, int depth, int remain)
                {
                    if (ix >= count)
                        return true;

                    for (var i = ix; i < count; i++)
                    {
                        var file = chunkFiles[i];
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
                    var file = chunkFiles[bestBuffer[i++]];
                    file.Location = position;
                    onProcess?.Invoke(file);
                    position += (uint)file.Size;
                }

                Console.WriteLine($"Chunk {gap.Start} - {gap.End} matched with {bestDepth} files {bestRemain:X} remaining");

                for (int i = bestDepth; i > 0;)
                    chunkFiles.RemoveAt(bestBuffer[--i]);

            }
        }

        private static List<ChunkFile> DiscoverFiles(string baseDir, IEnumerable<DbFile> files)
            => files.Select(file =>
            {
                string folder = null, extension = "bin";
                switch (file.Type)
                {
                    case BinType.Bitmap: folder = "graphics"; break;
                    case BinType.Palette: folder = "palettes"; extension = "pal"; break;
                    case BinType.Music: folder = "music"; extension = "bgm"; break;
                    case BinType.Tileset: folder = "tilesets"; extension = "set"; break;
                    case BinType.Tilemap: folder = "tilemaps"; extension = "map"; break;
                    case BinType.Meta17: break;
                    case BinType.Spritemap: folder = "spritemaps"; break;
                    case BinType.Sound: folder = "sounds"; extension = "sfx"; break;
                };

                var path = $"{file.Name}.{extension}";
                if (folder != null)
                    path = Path.Combine(folder, path);

                path = File.Exists(path) ? path : Path.Combine(baseDir, path);
                var size = (int)(new FileInfo(path).Length);

                if (file.Compressed)
                    size += 2;

                return new ChunkFile { File = file, Path = path, Size = size };
            }).OrderByDescending(x => x.Size).ToList();


        private static IEnumerable<DbGap> DiscoverAvailableSpace(DbRoot root, out IEnumerable<DbFile> files)
        {

            List<DbGap> gaps = new([new DbGap { Start = 0x200000u, End = 0x400000u }]);
            files = root.Files.Where(x => x.XRef?.Any() == true).ToList();

            void mergeGap(Location start, Location end)
            {
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

            //Merge the file ranges
            foreach (var file in files)
                mergeGap(file.Start, file.End);

            //Gerge gap ranges
            foreach (var gap in root.Gaps)
                mergeGap(gap.Start, gap.End);

            //Split gaps across chunk boundaries
            for (int i = 0; i < gaps.Count; i++)
            {
                var g = gaps[i];
                Location start = g.Start, end = g.End;
                var next = start + (ChunkSize - (start.Offset % ChunkSize));
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

        class FileDef
        {
            public Location Loc;
            public BinType Type;
            public List<Location> Refs = new();
        }

        public static unsafe void ReadMetaXrefs(byte* basePtr)
        {
            var fileMap = new Dictionary<Location, FileDef>();

            ushort id;
            var ptr = basePtr + 0xD8000;
            do
            {
                id = *(ushort*)ptr;
                ptr += 2;
                while (true)
                {
                    var cmd = *ptr++;
                    if (cmd == 0) break;

                    Location loc, xref;
                    BinType type;

                    switch (cmd)
                    {
                        case 2: ptr += sizeof(Meta2); continue;
                        case 0x13: ptr += sizeof(Meta13); continue;
                        case 0x14: ptr += sizeof(Meta14); continue;
                        case 0x15: ptr += sizeof(Meta15); continue;

                        case 4:
                            var m4 = (Meta4*)ptr;
                            loc = (Location)m4->Address;
                            type = BinType.Palette;
                            xref = (uint)((byte*)(&m4->Address) - basePtr);
                            ptr += sizeof(Meta4);
                            break;

                        case 0x11:
                            var m11 = (Meta11*)ptr;
                            loc = (Location)m11->Address;
                            type = BinType.Music;
                            xref = (uint)((byte*)(&m11->Address) - basePtr);
                            ptr += sizeof(Meta11);
                            break;

                        case 3:
                            var m3 = (Meta3*)ptr;
                            loc = (Location)m3->Address;
                            type = BinType.Bitmap;
                            xref = (uint)((byte*)(&m3->Address) - basePtr);
                            ptr += sizeof(Meta3);
                            break;

                        case 5:
                            var m5 = (Meta5*)ptr;
                            loc = (Location)m5->Address;
                            type = BinType.Tileset;
                            xref = (uint)((byte*)(&m5->Address) - basePtr);
                            ptr += sizeof(Meta5);
                            break;

                        case 6:
                            var m6 = (Meta6*)ptr;
                            loc = (Location)m6->Address;
                            type = BinType.Tilemap;
                            xref = (uint)((byte*)(&m6->Address) - basePtr);
                            ptr += sizeof(Meta6);
                            break;

                        case 0x10:
                            var m10 = (Meta10*)ptr;
                            loc = (Location)m10->Address;
                            type = BinType.Spritemap;
                            xref = (uint)((byte*)(&m10->Address) - basePtr);
                            ptr += sizeof(Meta10);
                            break;

                        case 0x17:
                            var m17 = (Meta17*)ptr;
                            loc = (Location)m17->Address;
                            type = BinType.Meta17;
                            xref = (uint)((byte*)(&m17->Address) - basePtr);
                            ptr += sizeof(Meta17);
                            break;

                        default: throw new Exception("Unknown meta type");
                    }

                    if (!fileMap.TryGetValue(loc, out FileDef def))
                        fileMap[loc] = def = new FileDef() { Loc = loc, Type = type };

                    def.Refs.Add(xref);
                }
            } while (id < 0xFF);

            var stringBuilder = new StringBuilder();

            foreach (var entry in fileMap.Values.OrderBy(x => x.Loc))
            {
                string folder = null, prefix = "", extension = "bin";
                switch (entry.Type)
                {
                    case BinType.Bitmap: folder = "graphics"; prefix = "bmp"; break;
                    case BinType.Palette: folder = "palettes"; prefix = "palette"; extension = "pal"; break;
                    case BinType.Music: folder = "music"; prefix = "bgm"; extension = "bgm"; break;
                    case BinType.Tileset: folder = "tilesets"; prefix = "set"; extension = "set"; break;
                    case BinType.Tilemap: folder = "tilemaps"; prefix = "map"; extension = "map"; break;
                    //case BinType.Meta17: break;
                    case BinType.Spritemap: folder = "spritemaps"; prefix = "sprite"; break;
                        //case BinType.Sound: folder = "sounds"; extension = "sfx"; break;
                };
                stringBuilder.AppendLine(
                    $@"{{ ""path"": ""{prefix}_{entry.Loc}.{extension}"", ""type"": ""{entry.Type}"", ""xref"": [ ""{string.Join("\", \"", entry.Refs)}"" ] }},"
                );
            }

            using (var file = File.Create("C:\\project.json"))
            using (var writer = new StreamWriter(file))
                writer.Write(stringBuilder.ToString());
        }
    }
}
