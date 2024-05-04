﻿using System.IO.MemoryMappedFiles;
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
using System.Text;
using System.Xml.Linq;

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

        public void AddInclude(DbPart part, Location loc)
        {
            if (part.Block.IsOutside(loc, out var p))
                part.Includes.Add(p);
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
                    part.Includes = new HashSet<DbPart>(); //Initialize part
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
                        bool isPtr = str[0] == '*', isAddr = str[0] == '&';
                        var otherStr = (isPtr || isAddr) ? str[1..] : (part.Struct ?? "Binary");

                        if (isPtr) str = "Offset";
                        else if (isAddr) str = "Address";

                        HexString? delimiter = null;

                        byte[] parseBinary()
                        {
                            var cur = lCur;

                            do Advance();
                            while (CanContinue());

                            return ReadBytes(cur.Offset, lCur.Offset - cur.Offset);
                        };

                        char[] parseString()
                        {
                            var dict = DbRoot.CommandStrings;
                            var builder = new StringBuilder();

                            do
                            {
                                var c = *Advance();
                                if (c == 0) break;

                                if (dict.TryGetValue(new(c), out var cmd))
                                {
                                    if (cmd.Types != null)
                                    {
                                        builder.Append($"[{cmd.Value}");

                                        bool first = true;
                                        foreach (var t in cmd.Types)
                                        {
                                            if (first) { builder.Append(':'); first = false; }
                                            else builder.Append(',');

                                            switch (t)
                                            {
                                                case MemberType.Byte: builder.Append($"{*Advance():X}"); break;
                                                case MemberType.Word: builder.Append($"{*(ushort*)Advance(2):X}"); break;

                                                case MemberType.Offset:
                                                    var loc = *(ushort*)Advance(2) | (lCur.Offset & 0x3F0000u);
                                                    goto writeloc;

                                                case MemberType.Address:
                                                    loc = *(ushort*)Advance(2) | ((uint)*Advance() << 16);
                                                writeloc:
                                                    builder.Append($"%{loc:X6}}}");
                                                    break;

                                                case MemberType.Binary:
                                                    bool sfirst = true;
                                                    do
                                                    {
                                                        var r = *Advance();
                                                        if (r == 0xFF) break;
                                                        if (sfirst) sfirst = false;
                                                        else builder.Append(',');
                                                        builder.Append($"{r:X}");
                                                    } while (CanContinue());
                                                    break;

                                                default: throw new("Unsupported member type");
                                            }
                                        }

                                        builder.Append(']');
                                    }
                                    else
                                        builder.Append(cmd.Value);
                                }
                                else
                                    builder.Append((char)c);
                            } while (CanContinue());

                            var chars = new char[builder.Length];
                            builder.CopyTo(0, chars, 0, builder.Length);
                            return chars;
                        };

                        Location parseLocation(Location loc)
                        {
                            if (part.IsInside(loc))
                            {
                                chunkTable.TryAdd(loc, otherStr);
                                RefList.TryAdd(loc, $"{otherStr.ToLower()}_{loc}");
                            }
                            return loc;
                        }

                        //Parse raw values
                        if (Enum.TryParse<MemberType>(str, true, out var mType))
                            return mType switch
                            {
                                MemberType.Byte => *Advance(1),
                                MemberType.Word => *(ushort*)Advance(2),
                                MemberType.Offset => parseLocation(*(ushort*)Advance(2) | baseLoc),
                                MemberType.Address => parseLocation(*(ushort*)Advance(2) | ((uint)*Advance(1) << 16)),
                                MemberType.Binary => parseBinary(),
                                MemberType.String => parseString(),
                                _ => throw new("Invalid member type"),
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
                            var parts = new object[members]; //Create new member collection
                            var def = new StructDef { Name = target.Name, Parts = parts };

                            //Parse each member of the struct
                            for (int i = 0; i < members; i++)
                                parts[i] = parseType(types[i]);

                            objects.Add(new StructDef { Name = target.Name, Parts = parts });

                            if (!CanContinue()) break;
                        }

                        return objects.ToArray();
                    }

                    switch (part.Type)
                    {
                        case PartType.Table:

                            bool isInit = true;
                            object current = null;
                            var locations = new List<Location>();
                            var chunks = new List<TableEntry>();
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
                                    locations.Add((Location)parseType("Offset"));
                                    continue;
                                }

                                var entry = new TableEntry(lCur);

                                if (current is string str)
                                    value = parseType(str);

                                entry.Object = value;
                                chunks.Add(entry);
                            }

                            part.ObjectRoot = new TableGroup()
                            {
                                Locations = locations.ToArray(),
                                Blocks = chunks.ToArray()
                            };


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
                        case PartType.Array:

                            void ResolveObject(object obj)
                            {
                                if (obj is string str) { }
                                else if (obj is IEnumerable arr)
                                    foreach (var o in arr)
                                        ResolveObject(o);
                                else if (obj is Location loc)
                                    AddInclude(part, loc);
                                else if (obj is StructDef sdef)
                                    ResolveObject(sdef.Parts);
                                else if (obj is TableEntry tab)
                                    ResolveObject(tab.Object);
                                else if (obj is TableGroup tgrp)
                                {
                                    ResolveObject(tgrp.Locations);
                                    ResolveObject(tgrp.Blocks);
                                }
                                //else if (obj is Tuple<Location, object> tup)
                                //    return new Tuple<string, object>(ResolveName(part, tup.Item1), tup.Item2);
                                //else if(obj is string str)
                                //{
                                //    for(var ix = str.IndexOf("%"); ix >= 0; ix = str.IndexOf("%"))
                                //    {
                                //        var sLoc = Location.Parse(str.Substring(ix + 1, 6));
                                //        str = str.Replace(str.Substring(ix, 7), ResolveName(part, sLoc));
                                //    }
                                //}

                                //return obj;
                            };

                            ResolveObject(part.ObjectRoot);

                            break;

                        default:
                            for (var op = part.Head; op != null; op = op.Next)
                                if (op.Operands != null)
                                    for (int i = 0; i < op.Operands.Length; i++)
                                        if (op.Operands[i] is Location loc)
                                            op.Operands[i] = ResolveName(part, loc);
                            //AddInclude(part, loc);
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
                    writer.WriteLine("include '{0}'", inc.Name); //Write includes

                writer.WriteLine(); //Empty line
                bool inBlock = false;

                foreach (var part in block.Parts) //Iterate over each part
                    switch (part.Type)
                    {
                        case PartType.Table:
                        case PartType.Array:

                            if (inBlock) writer.WriteLine("--------------------"); //Serparator
                            else inBlock = true;


                            bool isInline = true;
                            void WriteObject(object obj, int depth)
                            {
                                void Indent()
                                { for (int i = 0; i < depth; i++) writer.Write("  "); }

                                if (obj is TableGroup tGroup)
                                {
                                    WriteObject(tGroup.Locations, depth);
                                    writer.WriteLine();

                                    foreach (var t in tGroup.Blocks)
                                    {
                                        writer.Write($"{(RefList.TryGetValue(t.Location, out var s) ? s : $"loc_{t.Location}")} ");
                                        WriteObject(t.Object, depth);
                                        writer.WriteLine();
                                    }
                                    return;
                                }

                                if (!isInline)
                                {
                                    writer.WriteLine();
                                    Indent();
                                }

                                if (obj is StructDef sDef)
                                {
                                    writer.Write($"{sDef.Name} < ");
                                    isInline = true;
                                    bool first = true;
                                    foreach (var o in sDef.Parts)
                                    {
                                        if (first) first = false;
                                        else writer.Write(", ");
                                        WriteObject(o, depth);
                                    }
                                    writer.Write(" >");
                                    isInline = false;

                                }
                                else if (obj is Location l) writer.Write(ResolveName(part, l));
                                else if (obj is byte b) writer.Write("#{0:X2}", b);
                                else if (obj is ushort s) writer.Write("#{0:X4}", s);
                                else if (obj is byte[] a)
                                {
                                    writer.Write("#");
                                    writer.Write(Convert.ToHexString(a));
                                }
                                else if (obj is string[] sArr)
                                    foreach (var sa in sArr)
                                        WriteObject(sa, depth);
                                else if (obj is char[] c)
                                {
                                    writer.Write('`');
                                    writer.Write(c);
                                    writer.Write('`');
                                }
                                else if (obj is string str)
                                    writer.Write(str);
                                else if (obj is IEnumerable arr)
                                {
                                    writer.Write('[');
                                    isInline = false;
                                    foreach (var o in arr)
                                        WriteObject(o, depth + 1);

                                    writer.WriteLine();
                                    Indent();
                                    writer.Write(']');
                                    isInline = true;
                                }
                                else writer.Write(obj);

                                if (depth == 0) writer.WriteLine();

                            }

                            writer.Write($"{part.Name} "); //Label
                            WriteObject(part.ObjectRoot, 0);

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

        public class TableGroup
        {
            public IEnumerable<Location> Locations { get; set; }
            public IEnumerable<TableEntry> Blocks { get; set; }
        }

        public class TableEntry
        {
            public Location Location { get; set; }
            //public string Name { get; set; }
            public object Object { get; set; }

            public TableEntry() { }
            public TableEntry(Location loc) { Location = loc; }
        }

        public class StructDef
        {
            public string Name { get; set; }
            public object[] Parts { get; set; }
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
