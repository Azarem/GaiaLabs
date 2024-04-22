using System.IO.MemoryMappedFiles;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GaiaLabs
{
    public unsafe class RomLoader : IDisposable
    {
        public static RomLoader Current { get; set; }

        public RomMap Map { get; private set; }

        private MemoryMappedFile _mappedFile;
        private MemoryMappedViewAccessor _viewAccessor;
        public RomHeader Header;
        public byte* _baseAddress;
        public IntPtr _basePtr;
        public uint _offset;
        public DbRoot DbRoot;
        public Dictionary<Location, string> RefList = new();
        public Dictionary<Location, bool?> AccumulatorFlags = new();
        public Dictionary<Location, bool?> IndexFlags = new();
        //private bool _hasCopyHeader;

        public Dictionary<uint, DataEntry> Resources = new();

        protected RomLoader(string file)
        {
            LoadInternal(file);
        }

        protected void LoadInternal(string file)
        {
            long fileSize = 0;
            string extension = file.GetExtension();

            //void processZip(Stream inStream)
            //{
            //    var outStream = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 0, FileOptions.RandomAccess | FileOptions.DeleteOnClose);

            //    using (inStream)
            //        if (extension == "gz")
            //            GZip.Decompress(inStream, outStream, false);
            //        else
            //            inStream.CopyTo(outStream);

            //    outStream.Flush();
            //    outStream.Position = 0;
            //    fileSize = outStream.Length;

            //    _mappedFile = MemoryMappedFile.CreateFromFile(outStream, null, 0, MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, false);
            //};

            switch (extension)
            {
                //case "zip":
                //    using (var newFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(file))
                //        foreach (ZipEntry entry in newFile as IEnumerable)
                //            if ((extension = entry.Name.GetExtension()) == "smc" || extension == "sfc")
                //            {
                //                processZip(newFile.GetInputStream(entry.ZipFileIndex));
                //                break;
                //            }
                //    break;

                //case "gz":
                //    processZip(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));
                //    break;

                case "smc":
                case "sfc":
                    fileSize = new FileInfo(file).Length;
                    _mappedFile = MemoryMappedFile.CreateFromFile(file, FileMode.Open, null, 0, MemoryMappedFileAccess.ReadWrite);
                    break;

                default:
                    throw new("Unable to determine file type from extension.");
            }

            long cartHeader = fileSize % 0x400;

            if (cartHeader != 0 && cartHeader != 0x200)
                throw new("Invalid file size");

            _viewAccessor = _mappedFile.CreateViewAccessor(cartHeader, fileSize - cartHeader);
            _viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _baseAddress);

            _basePtr = (IntPtr)_baseAddress;
            Header = *(RomHeader*)(_baseAddress + 0xffc0);

            Current = this;
        }

        public static RomLoader Load(string file)
        {
            var loader = new RomLoader(file);
            loader.Map = new RomMap(loader);
            Event.TriggerLoaded(loader.Map);
            return loader;
        }

        public Location GetRelative(Location loc, byte size = 2, byte offset = 0, byte? bank = null)
            => size switch
            {
                2 => (Location)new Address(bank ?? loc.Bank, *(ushort*)(_baseAddress + loc)),
                3 => (Location)new Address(bank ?? *(_baseAddress + loc + offset + 2), *(ushort*)(_baseAddress + loc + offset)),
                _ => throw new("Invalid reference size")
            };

        public byte ReadByte() => ReadByte(_offset++);
        public byte ReadByte(uint offset) => _baseAddress[offset];
        public ushort ReadUInt16() => ReadUInt16((_offset += 2) - 2);

        public ushort ReadUInt16(uint offset) => *(ushort*)(_basePtr + offset);
        //public byte PeekByte() => ReadByte(_offset);

        public T ReadStruct<T>() where T : struct
        {
            var ptr = (T*)(_basePtr + _offset);
            _offset += (uint)sizeof(T);
            return *ptr;
        }

        public ICollection<T> ReadList<T>() where T : DataEntry, new()
        {
            var off = _offset;
            var list = new List<T>();
            var current = new T() { Location = off };
            while (current.Unpack(this))
            {
                current.Size = _offset - off;
                list.Add(current);
                if (!current.HasNext(this)) break;
                current = new T() { Location = off = _offset };
            }
            return list;
        }
        public ICollection<T> ReadList<T>(uint offset) where T : DataEntry, new()
        {
            _offset = offset;
            return ReadList<T>();
        }

        public T GetReference<T>(uint offset) where T : DataEntry, new()
        {
            if (!Resources.TryGetValue(offset, out var entry))
                Resources[offset] = entry = new T() { Location = offset };

            return entry as T;
        }

        private static void UpdateFlags<T>(IDictionary<Location, T?> dictionary, Location loc, T? value) where T : struct
        {
            if (dictionary.TryGetValue(loc, out var entry)) //Look for existing value
            {
                if (entry == null)
                    return; //Ignore entries with a hard unknown

                if (value != null && !value.Equals(entry)) //Check for disagreements
                    value = null; //Force hard unknown
                else if (value.Equals(entry))
                    return; //Ignore when value will not change
            }

            dictionary[loc] = value;
        }

        public string ResolveName(DbPart part, Location loc)
        {
            if (!RefList.TryGetValue(loc, out var name))
            {
                name = "loc_" + loc;
                RefList[loc] = name;
            }

            if (part.Block.IsOutside(loc, out var p))
            {
                part.Includes.Add(p);
                name = $"!{p.Block.Name}.{name}";
            }
            else if (!name.StartsWith("loc_"))
                name = $"@{name}";

            return name;
        }

        public void DumpDatabase(string outPath, string dbFile = "database.json")
        {
            using (var file = File.OpenRead(dbFile))
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };
                DbRoot = JsonSerializer.Deserialize<DbRoot>(file, options);
            }

            RefList = new();

            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts) //Analyze code and place markers
                {
                    part.Includes = new List<DbPart>(); //Initialize part
                    RefList[part.Start] = part.Name; //Add reference name

                    switch (part.Type)
                    {
                        case PartType.Table:

                            var table = new List<Location>();
                            var bank = part.Start.Offset & 0x3F0000u;
                            for (var loc = part.Start; loc < part.End; loc += 2)
                                table.Add(bank | *(ushort*)(_baseAddress + loc));
                            part.Table = [.. table];
                            break;

                        case PartType.Struct:
                            var struc = DbRoot.Structs[part.Struct];
                            var strList = new List<object[]>();
                            var types = struc.Types;
                            var members = types.Length;
                            var baseLoc = part.Start & 0x3F0000u;

                            for (byte* ptr = _baseAddress + part.Start, end = _baseAddress + part.End; ptr < end;)
                            {
                                var parts = new object[members];
                                for (int i = 0; i < members; i++)
                                    switch (types[i])
                                    {
                                        case MemberType.Byte: parts[i] = *ptr++; break;
                                        case MemberType.Word: parts[i] = *(ushort*)ptr; ptr += 2; break;
                                        case MemberType.Offset: parts[i] = *(ushort*)ptr | baseLoc; ptr += 2; break;
                                        case MemberType.Address:
                                            parts[i] = (Location)(*(ushort*)ptr | ((uint)ptr[2] << 16));
                                            ptr += 3;
                                            break;
                                        default: throw new("Invalid struct type");
                                    }
                                strList.Add(parts);
                            }

                            part.Structs = [.. strList];

                            break;

                        default:

                            var reg = new Registers();
                            Op prev = null;
                            for (var loc = part.Start; loc < part.End; loc += prev.Size)
                            {
                                //Process branch adjustments before parse
                                if (AccumulatorFlags.TryGetValue(loc, out var acc))
                                    reg.AccumulatorFlag = acc;
                                if (IndexFlags.TryGetValue(loc, out var ind))
                                    reg.IndexFlag = ind;

                                var op = Asm.Parse(_baseAddress, loc, reg, DbRoot); //Parse instruction

                                if (op.Code.Mnem == "SEP")
                                {
                                    var flag = (StatusFlags)op.Operands[0];
                                    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                                        UpdateFlags(AccumulatorFlags, loc + op.Size, true);
                                    if (flag.HasFlag(StatusFlags.IndexMode))
                                        UpdateFlags(IndexFlags, loc + op.Size, true);
                                }
                                else if (op.Code.Mnem == "REP")
                                {
                                    var flag = (StatusFlags)op.Operands[0];
                                    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                                        UpdateFlags(AccumulatorFlags, loc + op.Size, false);
                                    if (flag.HasFlag(StatusFlags.IndexMode))
                                        UpdateFlags(IndexFlags, loc + op.Size, false);
                                }

                                foreach (var obj in op?.Operands)
                                {
                                    if (obj is Location r)
                                    {
                                        if (reg.AccumulatorFlag != null)
                                            UpdateFlags(AccumulatorFlags, r, reg.AccumulatorFlag);
                                        if (reg.IndexFlag != null)
                                            UpdateFlags(IndexFlags, r, reg.IndexFlag);
                                    }
                                }

                                if (prev == null)
                                    part.Head = op; //Set head
                                else
                                    op.Prev = prev; //Set prev

                                prev = op; //Advance prev
                            }

                            break;
                    }
                }

            //Add code references
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
                    switch (part.Type)
                    {
                        case PartType.Table:
                            var tab = part.Table;
                            for (int i = 0; i < tab.Length; i++)
                                if (tab[i] is Location loc)
                                    tab[i] = ResolveName(part, loc); //Replace location with name
                            break;

                        case PartType.Struct:
                            foreach (var obj in part.Structs)
                                for (int i = 0; i < obj.Length; i++)
                                    if (obj[i] is Location loc)
                                        obj[i] = ResolveName(part, loc);
                            break;

                        default:
                            for (var op = part.Head; op != null; op = op.Next)
                                if (op.Operands != null)
                                    for (int i = 0; i < op.Operands.Length; i++)
                                        if (op.Operands[i] is Location loc)
                                            op.Operands[i] = ResolveName(part, loc);
                            break;
                    }

            //Write files
            foreach (var block in DbRoot.Blocks)
            {
                var outFile = Path.Combine(outPath, block.Name + ".asm");
                using var outStream = File.Create(outFile);
                using var writer = new StreamWriter(outStream);

                foreach (var inc in block.GetIncludes())
                    writer.WriteLine("#include '{0}'", inc.Name); //Write includes

                writer.WriteLine(); //Empty line
                bool inBlock = false;

                foreach (var part in block.Parts) //Iterate over each part
                    switch (part.Type)
                    {
                        case PartType.Table:

                            if (inBlock) writer.WriteLine("--------------------"); //Serparator
                            else inBlock = true;

                            writer.WriteLine($"{part.Name} ["); //Label

                            foreach (var obj in part.Table)
                                writer.WriteLine($"  {obj}"); //Location names

                            writer.WriteLine(']'); //End
                            break;

                        case PartType.Struct:

                            if (inBlock) writer.WriteLine("--------------------"); //Serparator
                            else inBlock = true;

                            writer.WriteLine($"{part.Name} ["); //Label

                            foreach (var obj in part.Structs)
                            {
                                writer.Write($"  {part.Struct} < ");
                                bool begin = true;
                                foreach(var mem in obj)
                                {
                                    if (begin) begin = false;
                                    else writer.Write(", ");

                                    if (mem is byte b) writer.Write("#{0:X2}", b);
                                    else if (mem is ushort s) writer.Write("#{0:X4}", s);
                                    else writer.Write(mem);
                                }
                                writer.WriteLine(" >");
                            }

                            writer.WriteLine(']'); //End
                            break;

                        default:
                            bool first = true;
                            for (var op = part.Head; op != null; op = op.Next) //Process each instruction in sequence
                            {
                                if (RefList.TryGetValue(op.Location, out var label)) //Check for code label
                                {
                                    if (first)
                                    {
                                        if (inBlock)
                                            writer.WriteLine("--------------------");
                                        writer.WriteLine("{0} {{", label); //Write label
                                        first = false;
                                    }
                                    else
                                    {
                                        writer.WriteLine();
                                        writer.WriteLine("  {0}:", label); //Write label
                                    }
                                    inBlock = true;
                                }
                                writer.WriteLine("    {0}", op); //Write instruction
                            }
                            writer.WriteLine("}");
                            break;
                    }
            }
        }

        public void Dispose()
        {
            if (_mappedFile != null)
            {
                _mappedFile.Dispose();
                _mappedFile = null;
            }
            GC.SuppressFinalize(this);
        }


        public enum SeekSet
        {
            Beginning = 0,
            Current = 1,
            End = 2
        }


        public enum HeaderField
        {
            CartName = 0x00,
            Mapper = 0x15,
            RomType = 0x16,
            RomSize = 0x17,
            RamSize = 0x18,
            CartRegion = 0x19,
            Company = 0x1a,
            Version = 0x1b,
            Complement = 0x1c,  //inverse checksum
            Checksum = 0x1e,
            ResetVector = 0x3c,
        };

        public enum CartType
        {
            TypeNormal = 0,
            TypeBsxSlotted,
            TypeBsxBios,
            TypeBsx,
            TypeSufamiTurboBios,
            TypeSufamiTurbo,
            TypeSuperGameBoy1Bios,
            TypeSuperGameBoy2Bios,
            TypeGameBoy,
            TypeUnknown,
        };

        public enum Region
        {
            NTSC = 0,
            PAL,
        };

        public enum MemoryMapper
        {
            LoROM = 0,
            HiROM,
            ExLoROM,
            ExHiROM,
            SuperFXROM,
            SA1ROM,
            SPC7110ROM,
            BSCLoROM,
            BSCHiROM,
            BSXROM,
            STROM,
        };

        public enum DSP1MemoryMapper
        {
            DSP1Unmapped = 0,
            DSP1LoROM1MB,
            DSP1LoROM2MB,
            DSP1HiROM,
        };
    }

}
