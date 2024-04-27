using System.IO.MemoryMappedFiles;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using Godot;
using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.InteropServices;

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

        public byte[] ReadBytes(uint offset, uint len) => ReadBytes(_baseAddress + offset, len);
        public static byte[] ReadBytes(byte* ptr, uint len)
        {
            var arr = new byte[len];
            Marshal.Copy((nint)ptr, arr, 0, (int)len);
            return arr;
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
                name = $"loc_{loc}";
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

        public void AnalyzeBlocks()
        {
            //Read and analyze data/code and place markers
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
                {
                    part.Includes = new List<DbPart>(); //Initialize part
                    RefList[part.Start] = part.Name; //Add reference name

                    Location lCur = part.Start, lEnd = part.End;
                    byte* pCur = _baseAddress + lCur, pEnd = _baseAddress + lEnd;
                    var baseLoc = lCur & 0x3F0000u;
                    var chunkTable = new Dictionary<Location, object>();

                    byte* Advance(uint count = 1, bool flag = true)
                    {
                        var orig = pCur;
                        if (flag) { pCur += count; lCur += count; }
                        return orig;
                    }

                    object parseType(string str)
                    {
                        HexString? delimiter = null;

                        byte[] parseBinary()
                        {
                            var cur = lCur;

                            do Advance();
                            while (CanContinue());

                            return ReadBytes(cur.Offset, lCur.Offset - cur.Offset);
                        };

                        //Parse raw values
                        if (Enum.TryParse<MemberType>(str, true, out var mType))
                            return mType switch
                            {
                                MemberType.Byte => (object)*Advance(1),
                                MemberType.Word => *(ushort*)Advance(2),
                                MemberType.Offset => *(ushort*)Advance(2) | baseLoc,
                                MemberType.Address => (Location)(*(ushort*)Advance(2) | ((uint)*Advance(1) << 16)),
                                MemberType.Binary => parseBinary(),
                                _ => throw new("Invalid struct type"),
                            };

                        var parent = DbRoot.Structs[str];
                        delimiter = parent.Delimiter;
                        var descriminator = parent.Descriminator;
                        var objects = new List<object>();

                        bool DelimiterReached()
                        {
                            if (delimiter != null)
                                switch (delimiter.Value.TypeCode)
                                {
                                    case TypeCode.Byte:
                                        if (*pCur == delimiter.Value.Value)
                                        { Advance(); return true; }
                                        break;
                                    case TypeCode.UInt16:
                                        if (*(ushort*)pCur == delimiter.Value.Value)
                                        { Advance(2); return true; }
                                        break;
                                    case TypeCode.UInt32:
                                        if ((*(ushort*)pCur | ((uint)pCur[2] << 16)) == delimiter.Value.Value)
                                        { Advance(3); return true; }
                                        break;
                                    default: throw new("Type code not supported");
                                }
                            return false;
                        }

                        bool CanContinue()
                        {
                            if (pCur >= pEnd /*|| lCur >= lEnd*/) return false;
                            if (chunkTable.ContainsKey(lCur)) return false;
                            return true;
                        }


                        //Continue to iterate until end or delimiter is reached
                        while (!DelimiterReached())
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
                                        desc = pCur[offset];
                                        if (isFirst) Advance();
                                        break;
                                    case TypeCode.UInt16:
                                        desc = *(ushort*)(pCur + offset);
                                        if (isFirst) Advance(2);
                                        break;
                                    case TypeCode.UInt32:
                                        desc = *(ushort*)(pCur + offset) | ((uint)pCur[offset + 2] << 16);
                                        if (isFirst) Advance(3);
                                        break;
                                    default: throw new("Type code not supported");
                                }

                                //Match descriminator to type
                                target = DbRoot.Structs.FirstOrDefault(x => x.Value.Parent == str && x.Value.Descriminator == desc).Value
                                    ?? parent;// throw new($"Could not find type for descriminator {desc}");
                            }

                            var types = target.Types;
                            var members = types.Length;
                            var parts = new object[members + 1]; //Create new member collection
                            parts[0] = target.Name; //First object is type name

                            //Parse each member of the struct
                            for (int i = 0; i < members; i++)
                            {
                                //var res = parseType(types[i]);
                                //if (res is object[] arr && arr[0] is not string)
                                //    objects.AddRange(arr);
                                //else
                                //    objects.a
                                parts[i + 1] = parseType(types[i]);
                            }

                            objects.Add(parts);

                            if (!CanContinue()) break;
                        }

                        return objects.ToArray();
                    }

                    switch (part.Type)
                    {
                        case PartType.Table:

                            //var table = new Dictionary<Location, object>();
                            //var stru = part.Struct != null ? DbRoot.Structs[part.Struct] : null;
                            bool isInit = true;
                            object current = null;
                            //var result = new List<object>();
                            var locations = new List<Tuple<Location, object>>();
                            int initCount = 0;
                            while (pCur < pEnd)
                            {
                                //Look for chunk at current location
                                if (chunkTable.TryGetValue(lCur, out var value))
                                {
                                    if (isInit)
                                    {
                                        initCount = locations.Count;
                                        isInit = false;
                                    }
                                    current = value;
                                }
                                else if (isInit)
                                {
                                    var offset = *(ushort*)Advance(2) | baseLoc;
                                    chunkTable[offset] = part.Struct ?? "Binary";
                                    locations.Add(new(offset, null));
                                    continue;
                                }

                                var ix = locations.IndexOf(new(lCur, null));
                                var lOld = lCur;

                                if (current is string str)
                                    value = parseType(str);

                                if (ix < 0)
                                    locations.Add(new(lOld, new Wrapper(value)));
                                else if (value != null)
                                {
                                    var l = locations[ix].Item1;
                                    locations[ix] = new(l, value);
                                    RefList.TryAdd(l, $"chunk_{l}");
                                }

                                void FindData(object o)
                                {
                                    if (o is not string && o is IEnumerable e)
                                        foreach (var x in e)
                                            FindData(x);
                                    else if (o is Location loc && part.IsInside(loc))
                                    {
                                        chunkTable.TryAdd(loc, "Binary");
                                        RefList.TryAdd(loc, $"binary_{loc}");
                                    }
                                }

                                FindData(value);

                            }

                            part.ObjectRoot = part.Struct == null
                                ? locations.Select(x => x.Item1).ToArray()
                                : locations.ToArray();

                            break;

                        case PartType.Array:
                            var strList = new List<object>();

                            while (pCur < pEnd)
                            {
                                var res = parseType(part.Struct);
                                if (res is object[] arr && arr[0] is not string)
                                    strList.AddRange(arr);
                                else
                                    strList.Add(res);
                            }

                            part.ObjectRoot = strList.ToArray();

                            break;

                        default:

                            var reg = new Registers();
                            Op prev = null;
                            while (lCur < lEnd)
                            {
                                //Process branch adjustments before parse
                                if (AccumulatorFlags.TryGetValue(lCur, out var acc))
                                    reg.AccumulatorFlag = acc;
                                if (IndexFlags.TryGetValue(lCur, out var ind))
                                    reg.IndexFlag = ind;

                                var op = Asm.Parse(_baseAddress, lCur, reg, DbRoot); //Parse instruction

                                if (op.Code.Mnem == "SEP")
                                {
                                    var flag = (StatusFlags)op.Operands[0];
                                    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                                        UpdateFlags(AccumulatorFlags, lCur + op.Size, true);
                                    if (flag.HasFlag(StatusFlags.IndexMode))
                                        UpdateFlags(IndexFlags, lCur + op.Size, true);
                                }
                                else if (op.Code.Mnem == "REP")
                                {
                                    var flag = (StatusFlags)op.Operands[0];
                                    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                                        UpdateFlags(AccumulatorFlags, lCur + op.Size, false);
                                    if (flag.HasFlag(StatusFlags.IndexMode))
                                        UpdateFlags(IndexFlags, lCur + op.Size, false);
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

                                Advance(op.Size); //Advance location
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
                        //var tab = part.Table;
                        //for (int i = 0; i < tab.Length; i++)
                        //    if (tab[i] is Location loc)
                        //        tab[i] = ResolveName(part, loc); //Replace location with name
                        //break;


                        case PartType.Array:

                            //if(part.ObjectRoot is Location[] lArr)
                            //{
                            //    foreach (var l in lArr)
                            //        RefList.TryAdd(l, $"chunk_{l}");
                            //}

                            //object ResolveObject(object obj)
                            //{
                            //    if (obj is Location[] lArr)
                            //    {
                            //        var oArr = new object[lArr.Length];
                            //        for (int i = 0; i < lArr.Length; i++)
                            //        {
                            //            var l = lArr[i];
                            //            oArr[i] = l;
                            //            RefList.TryAdd(l, $"chunk_{l}");
                            //        }
                            //        return oArr;
                            //    }
                            //    else if (obj is object[] arr)
                            //        for (int i = 0; i < arr.Length; i++)
                            //            arr[i] = ResolveObject(arr[i]);
                            //    else if (obj is Location loc)
                            //        return ResolveName(part, loc);
                            //    else if (obj is Tuple<Location, object> tup)
                            //        return new Tuple<string, object>(ResolveName(part, tup.Item1), tup.Item2);

                            //    return obj;
                            //};

                            //Process each member to resolve location names
                            //var arr = part.ObjectRoot;
                            //for (int i = 0; i < arr.Length; i++)
                            //    arr[i] = ResolveObject(arr[i]);

                            //part.ObjectRoot = ResolveObject(part.ObjectRoot);

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

                        //if (inBlock) writer.WriteLine("--------------------"); //Serparator
                        //else inBlock = true;

                        //writer.WriteLine($"{part.Name} ["); //Label

                        //foreach (var obj in part.Table)
                        //    writer.WriteLine($"  {obj}"); //Location names

                        //writer.WriteLine(']'); //End
                        //break;

                        case PartType.Array:

                            if (inBlock) writer.WriteLine("--------------------"); //Serparator
                            else inBlock = true;

                            writer.WriteLine($"{part.Name} ["); //Label

                            int inlineDepth = 0;
                            void WriteObject(object obj, int depth)
                            {
                                void Indent()
                                { for (int i = 0; i < depth; i++) writer.Write("  "); }

                                if (obj is Wrapper w)
                                    obj = w.Obj;

                                //bool inChunk = false;
                                if (obj is Tuple<Location, object>[] tArr)
                                {
                                    //inChunk = true;
                                    //depth++;
                                    //Write table
                                    WriteObject(
                                        tArr.Where(x => x.Item2 is not Wrapper)
                                            .Select(x => x.Item1).ToArray(),
                                        depth + 1);
                                    //foreach (var t in tArr)
                                    //{ Indent(); WriteObject(t.Item1, depth + 1); }
                                    Indent();
                                    writer.WriteLine(']');
                                    writer.WriteLine();

                                    foreach (var t in tArr.Where(x => x.Item2 != null).OrderBy(x => x.Item1.Offset))
                                    {
                                        //writer.WriteLine($"{ResolveName(part, t.Item1)} [");
                                        writer.Write($"{ResolveName(part, t.Item1)} ");
                                        WriteObject(t.Item2, depth + 1);
                                        //Indent();
                                        //writer.WriteLine(']');
                                        writer.WriteLine();
                                    }
                                    return;
                                }
                                //if (obj is Tuple<Location, object> tup)
                                //{
                                //}

                                if (obj is Location loc)
                                    obj = ResolveName(part, loc);


                                if (obj is Location[] lArr)
                                    foreach (var l in lArr)
                                    { WriteObject(l, depth + 1); writer.WriteLine(); }
                                else
                                {
                                    if (inlineDepth == 0)
                                        Indent();

                                    if (obj is object[] arr)
                                    {
                                        if (arr.Length == 0 || arr[0] is not string) //Check for flat object list TODO: Better way?
                                        {
                                            if (depth > 0) writer.WriteLine('[');
                                            foreach (var o in arr)
                                                WriteObject(o, depth + 1);

                                            if (depth > 0)
                                            {
                                                Indent();
                                                writer.Write(']');
                                            }
                                        }
                                        else
                                        {
                                            //Indent();
                                            writer.Write($"{arr[0]} < ");
                                            inlineDepth++;
                                            //writer.Write("< ");
                                            for (int i = 1; i < arr.Length; i++)
                                            {
                                                if (i > 1) writer.Write(", ");
                                                WriteObject(arr[i], depth + 1);
                                            }
                                            inlineDepth--;
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

                                //if (inChunk)
                                //{
                                //    inChunk = false;
                                //    writer.WriteLine();
                                //    depth--;
                                //}



                            }

                            //foreach (var obj in part.Objects)
                            WriteObject(part.ObjectRoot, 0);

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

        public readonly struct Wrapper
        {
            public readonly object Obj;

            public Wrapper(object o) { Obj = o; }
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
