using System.IO.MemoryMappedFiles;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;

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
            DbRoot = DbRoot.FromFile(dbFile);
            RefList = new();

            ExtractFiles(outPath);
            ExtractObjects(outPath);
            AnalyzeBlocks();
            ResolveReferences();
            WriteBlocks(outPath);
        }

        public void ExtractFiles(string outPath)
        {
            foreach (var file in DbRoot.Files)
            {
                RefList[file.Start] = file.Name;

                using var fileStream = File.Create(Path.Combine(outPath, file.Name + ".bin"));

                if (file.Compressed)
                {
                    var data = Compression.Expand(_basePtr + (nint)file.Start.Offset);
                    fixed (byte* dPtr = data)
                        for (byte* ptr = dPtr, end = ptr + data.Length; ptr < end; ptr++)
                            fileStream.WriteByte(*ptr);
                }
                else
                    for (byte* ptr = _baseAddress + file.Start, end = _baseAddress + file.End; ptr < end; ptr++)
                        fileStream.WriteByte(*ptr);
            }
        }

        public void ExtractObjects(string outPath)
        {
            foreach (var block in DbRoot.Objects)
            {
                RefList[block.Start] = block.Name;

            }
        }

        public void AnalyzeBlocks()
        {
            //Read and analyze data/code and place markers
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
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

                        case PartType.Array:
                            var strList = new List<object>();
                            var baseLoc = part.Start & 0x3F0000u;

                            for (byte* ptr = _baseAddress + part.Start, end = _baseAddress + part.End; ptr < end;)
                            {
                                object parseType(string str)
                                {
                                    object value;
                                    if (Enum.TryParse<MemberType>(str, true, out var mType))
                                    {
                                        switch (mType)
                                        {
                                            case MemberType.Byte: value = *ptr++; break;
                                            case MemberType.Word: value = *(ushort*)ptr; ptr += 2; break;
                                            case MemberType.Offset: value = *(ushort*)ptr | baseLoc; ptr += 2; break;
                                            case MemberType.Address:
                                                value = (Location)(*(ushort*)ptr | ((uint)ptr[2] << 16));
                                                ptr += 3;
                                                break;
                                            default: throw new("Invalid struct type");
                                        }
                                        return value;
                                    }

                                    var parent = DbRoot.Structs[str];
                                    var delimiter = parent.Delimiter;
                                    var descriminator = parent.Descriminator;
                                    var objects = new List<object>();

                                    bool ShouldContinue()
                                    {
                                        if (ptr >= end) return false;

                                        if (delimiter != null)
                                            switch (delimiter.Value.TypeCode)
                                            {
                                                case TypeCode.Byte:
                                                    if (*ptr == delimiter.Value.Value)
                                                    { ptr++; return false; }
                                                    break;
                                                case TypeCode.UInt16:
                                                    if (*(ushort*)ptr == delimiter.Value.Value)
                                                    { ptr += 2; return false; }
                                                    break;
                                                case TypeCode.UInt32:
                                                    if ((*(ushort*)ptr | ((uint)ptr[2] << 16)) == delimiter.Value.Value)
                                                    { ptr += 3; return false; }
                                                    break;
                                                default: throw new("Type code not supported");
                                            }

                                        return true;
                                    }


                                    do
                                    {
                                        var target = parent;
                                        if (descriminator != null) //Is composite?
                                        {
                                            //Get descriminator value
                                            var offset = descriminator.Value;
                                            var isFirst = offset == 0u;
                                            uint desc;
                                            switch (offset.TypeCode)
                                            {
                                                case TypeCode.Byte:
                                                    desc = ptr[offset];
                                                    if (isFirst) ptr++;
                                                    break;
                                                case TypeCode.UInt16:
                                                    desc = *(ushort*)(ptr + offset);
                                                    if (isFirst) ptr += 2;
                                                    break;
                                                case TypeCode.UInt32:
                                                    desc = *(ushort*)(ptr + offset) | ((uint)ptr[offset + 2] << 16);
                                                    if (isFirst) ptr += 3;
                                                    break;
                                                default: throw new("Type code not supported");
                                            }

                                            //Match descriminator to type
                                            target = DbRoot.Structs.FirstOrDefault(x => x.Value.Parent == str && x.Value.Descriminator == desc).Value
                                                ?? throw new($"Could not find type for descriminator {desc}");
                                        }

                                        var types = target.Types;
                                        var members = types.Length;
                                        var parts = new object[members + 1]; //Create new member collection
                                        parts[0] = target.Name; //First object is type name

                                        //Parse each member of the struct
                                        for (int i = 0; i < members; i++)
                                        {
                                            parts[i + 1] = parseType(types[i]);
                                        }

                                        objects.Add(parts);

                                    } while (ShouldContinue()); //Continue to iterate until end or delimiter is reached

                                    return objects.ToArray();
                                }

                                var res = parseType(part.Struct);
                                if (res is object[] arr && arr[0] is not string)
                                    strList.AddRange(arr);
                                else
                                    strList.Add(res);
                            }

                            part.Objects = [.. strList];

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

        }

        public void ResolveReferences()
        {
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


                        case PartType.Array:

                            object ResolveObject(object obj)
                            {
                                if (obj is object[] arr)
                                    for (int i = 0; i < arr.Length; i++)
                                        arr[i] = ResolveObject(arr[i]);
                                else if (obj is Location loc)
                                    return ResolveName(part, loc);

                                return obj;
                            };

                            //Process each member to resolve location names
                            var arr = part.Objects;
                            for (int i = 0; i < arr.Length; i++)
                                arr[i] = ResolveObject(arr[i]);

                            break;

                        default:
                            for (var op = part.Head; op != null; op = op.Next)
                                if (op.Operands != null)
                                    for (int i = 0; i < op.Operands.Length; i++)
                                        if (op.Operands[i] is Location loc)
                                            op.Operands[i] = ResolveName(part, loc);
                            break;
                    }
        }

        public void WriteBlocks(string outPath)
        {
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

                        case PartType.Array:

                            if (inBlock) writer.WriteLine("--------------------"); //Serparator
                            else inBlock = true;

                            writer.WriteLine($"{part.Name} ["); //Label


                            void WriteObject(object obj, int depth)
                            {
                                void Indent()
                                { for (int i = 0; i < depth; i++) writer.Write("  "); }

                                if (obj is object[] arr)
                                {
                                    if (arr[0] is not string) //Check for flat object list TODO: Better way?
                                    {
                                        if(depth > 0) writer.WriteLine('[');
                                        foreach (var o in arr)
                                            WriteObject(o, depth + 1);

                                        Indent();
                                        writer.Write(']');
                                    }
                                    else
                                    {
                                        Indent();
                                        writer.Write($"{arr[0]} < ");

                                        //writer.Write("< ");
                                        for (int i = 1; i < arr.Length; i++)
                                        {
                                            if (i > 1) writer.Write(", ");
                                            WriteObject(arr[i], depth + 1);
                                        }
                                        writer.WriteLine(" >");
                                    }
                                }
                                else if (obj is byte b) writer.Write("#{0:X2}", b);
                                else if (obj is ushort s) writer.Write("#{0:X4}", s);
                                else if (obj is byte[] a)
                                {
                                    writer.Write("#");
                                    writer.Write(Convert.ToHexString(a));
                                }
                                else writer.Write(obj);

                                if (depth == 0) writer.WriteLine();
                            }

                            foreach (var obj in part.Objects)
                                WriteObject(obj, 1);

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
                                        if (inBlock) writer.WriteLine("--------------------");
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
