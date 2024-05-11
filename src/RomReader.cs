using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GaiaLabs
{
    public unsafe class RomReader
    {
        const char RefChar = '~';

        public DbRoot DbRoot { get; set; }
        public Dictionary<Location, string> RefList = new();
        public Dictionary<Location, bool?> AccumulatorFlags = new();
        public Dictionary<Location, bool?> IndexFlags = new();

        private byte* _basePtr;
        private byte* _pCur, _pEnd;
        private Location _lCur, _lEnd;
        private DbPart _part;
        private bool _isInline;
        private Dictionary<Location, string> _chunkTable = new();

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

        private byte* Advance(uint count = 1)
        {
            var orig = _pCur;
            _pCur += count;
            _lCur += count;
            return orig;
        }

        public string ResolveName(Location loc)
        {
            if (!RefList.TryGetValue(loc, out var name))
            {
                name = $"loc_{loc}";
                RefList[loc] = name;
            }

            if (_part.Block.IsOutside(loc, out var p))
            {
                _part.Includes.Add(p);
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

        public DbRoot DumpDatabase(byte* basePtr, string outPath, string dbFile)
        {
            _basePtr = basePtr;
            DbRoot = DbRoot.FromFile(dbFile);

            ExtractFiles(outPath);
            AnalyzeBlocks();
            ResolveReferences();
            WriteBlocks(outPath);

            return DbRoot;
        }


        private void ExtractFiles(string outPath)
        {
            foreach (var file in DbRoot.Files)
            {
                RefList[file.Start] = file.Name;

                using var fileStream = File.Create(Path.Combine(outPath, file.Name + ".bin"));

                if (file.Compressed)
                {
                    var data = Compression.Expand(_basePtr + file.Start);
                    fixed (byte* dPtr = data)
                        for (byte* ptr = dPtr, end = ptr + data.Length; ptr < end; ptr++)
                            fileStream.WriteByte(*ptr);
                }
                else
                    for (byte* ptr = _basePtr + file.Start, end = _basePtr + file.End; ptr < end; ptr++)
                        fileStream.WriteByte(*ptr);
            }
        }

        private bool DelimiterReached(HexString? delimiter)
        {
            if (delimiter != null)
                switch (delimiter.Value.TypeCode)
                {
                    case TypeCode.Byte:
                        if (*_pCur == delimiter.Value.Value)
                        { Advance(); return true; }
                        break;
                    case TypeCode.UInt16:
                        if (*(ushort*)_pCur == delimiter.Value.Value)
                        { Advance(2); return true; }
                        break;
                    case TypeCode.UInt32:
                        if ((*(ushort*)_pCur | ((uint)_pCur[2] << 16)) == delimiter.Value.Value)
                        { Advance(3); return true; }
                        break;
                    default: throw new("Type code not supported");
                }
            return false;
        }

        private bool CanContinue()
        {
            if (_pCur >= _pEnd /*|| _lCur >= _lEnd*/) return false;
            if (_chunkTable.ContainsKey(_lCur)) return false;
            return true;
        }

        private byte[] ParseBinary()
        {
            var cur = _pCur;

            do Advance();
            while (CanContinue());

            var len = _pCur - cur;
            var bytes = new byte[len];

            for (int i = 0; i < len; i++)
                bytes[i] = cur[i];

            return bytes;
        }

        private void ResolveCommand(DbStringCommand cmd, StringBuilder builder)
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
                            var loc = *(ushort*)Advance(2) | (_lCur.Offset & 0x3F0000u);
                            goto writeloc;

                        case MemberType.Address:
                            loc = *(ushort*)Advance(2) | ((uint)*Advance() << 16);
                        writeloc:
                            builder.Append($"{RefChar}{loc:X6}");
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

        private string ParseString()
        {
            var dict = DbRoot.StringCommands;
            var builder = new StringBuilder();

            do
            {
                var c = *Advance();
                if (c == 0) break;

                if (dict.TryGetValue(new(c), out var cmd))
                    ResolveCommand(cmd, builder);
                else
                    builder.Append((char)c);
            } while (CanContinue());

            //var chars = new char[builder.Length];
            //builder.CopyTo(0, chars, 0, builder.Length);
            return builder.ToString();
        }

        private string ParseCompString()
        {
            var builder = new StringBuilder();

            do
            {
                var c = *Advance();
                if (c == 0xCA)
                    break;

                //var flag = c & 0x08;
                var index = (c & 0x70) >> 1 | (c & 0x07);
                builder.Append(DbRoot.CharMap[index]);
            } while (CanContinue());

            return builder.ToString();
        }

        private string ParseWideString()
        {
            var builder = new StringBuilder();
            var dict = DbRoot.WideCommands;

            do
            {
                var c = *Advance();
                if (c == 0xCA)
                    break;

                if (dict.TryGetValue(c, out var cmd))
                    ResolveCommand(cmd, builder);
                else
                {
                    var index = (c & 0xE0) >> 1 | (c & 0x0F);
                    builder.Append(DbRoot.WideMap[index]);
                }
            } while (CanContinue());

            return builder.ToString();
        }

        private Op ParseCode()
        {
            var reg = new Registers();
            Op prev = null, head = null;
            bool first = true;
            while (_lCur < _lEnd)
            {
                if (first) first = false;
                else if (_chunkTable.ContainsKey(_lCur)) break;

                //Process branch adjustments before parse
                if (AccumulatorFlags.TryGetValue(_lCur, out var acc))
                    reg.AccumulatorFlag = acc;
                if (IndexFlags.TryGetValue(_lCur, out var ind))
                    reg.IndexFlag = ind;

                var op = Asm.Parse(_basePtr, _lCur, reg, DbRoot); //Parse instruction

                if (op.Code.Mnem == "SEP")
                {
                    var flag = (StatusFlags)op.Operands[0];
                    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                        UpdateFlags(AccumulatorFlags, _lCur + op.Size, true);
                    if (flag.HasFlag(StatusFlags.IndexMode))
                        UpdateFlags(IndexFlags, _lCur + op.Size, true);
                }
                else if (op.Code.Mnem == "REP")
                {
                    var flag = (StatusFlags)op.Operands[0];
                    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                        UpdateFlags(AccumulatorFlags, _lCur + op.Size, false);
                    if (flag.HasFlag(StatusFlags.IndexMode))
                        UpdateFlags(IndexFlags, _lCur + op.Size, false);
                }

                for (var i = 0; i < op.Operands.Length; i++)
                {
                    var obj = op.Operands[i];
                    if (obj is Location r)
                    {
                        if (_part.IsInside(r))
                        {
                            if (op.CopDef != null)
                            {
                                var type = "Binary";
                                type = op.CopDef.Parts[i - 1];
                                if (type[0] == '*' || type[0] == '&')
                                    type = type[1..];
                                _chunkTable.TryAdd(r, type);
                            }
                        }

                        if (reg.AccumulatorFlag != null)
                            UpdateFlags(AccumulatorFlags, r, reg.AccumulatorFlag);
                        if (reg.IndexFlag != null)
                            UpdateFlags(IndexFlags, r, reg.IndexFlag);
                    }
                }

                if (prev == null)
                    head = op; //Set head
                else
                    op.Prev = prev; //Set prev

                Advance(op.Size); //Advance location
                prev = op; //Advance prev
            }

            return head;
        }

        private Location ParseLocation(Location loc, string otherStr)
        {
            if (_part.IsInside(loc))
            {
                _chunkTable.TryAdd(loc, otherStr);
                RefList.TryAdd(loc, $"{otherStr.ToLower()}_{loc}");
            }
            return loc;
        }

        private object ParseType(string str)
        {
            bool isPtr = str[0] == '*', isAddr = str[0] == '&';
            var otherStr = (isPtr || isAddr) ? str[1..] : (_part.Struct ?? "Binary");

            if (isPtr) str = "Offset";
            else if (isAddr) str = "Address";

            //Parse raw values
            if (Enum.TryParse<MemberType>(str, true, out var mType))
                return mType switch
                {
                    MemberType.Byte => *Advance(1),
                    MemberType.Word => *(ushort*)Advance(2),
                    MemberType.Offset => ParseLocation(*(ushort*)Advance(2) | (_lCur.Offset & 0x3F0000u), otherStr),
                    MemberType.Address => ParseLocation(*(ushort*)Advance(2) | ((uint)*Advance(1) << 16), otherStr),
                    MemberType.Binary => ParseBinary(),
                    MemberType.String => ParseString(),
                    MemberType.CompString => ParseCompString(),
                    MemberType.WideString => ParseWideString(),
                    MemberType.Code => ParseCode(),
                    _ => throw new("Invalid member type"),
                };

            var parent = DbRoot.Structs[str];
            var delimiter = parent.Delimiter;
            var descriminator = parent.Descriminator;
            var objects = new List<object>();

            //Continue to iterate until end or delimiter is reached
            while (!DelimiterReached(delimiter))
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
                            desc = _pCur[offset];
                            if (isFirst) Advance();
                            break;
                        case TypeCode.UInt16:
                            desc = *(ushort*)(_pCur + offset);
                            if (isFirst) Advance(2);
                            break;
                        case TypeCode.UInt32:
                            desc = *(ushort*)(_pCur + offset) | ((uint)_pCur[offset + 2] << 16);
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
                    parts[i] = ParseType(types[i]);

                objects.Add(def);

                if (!CanContinue()) break;
            }

            return objects;
        }

        private void AnalyzeBlocks()
        {
            //Read and analyze data/code and place markers
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
                {
                    _part = part;
                    _lCur = part.Start;
                    _lEnd = part.End;
                    _pCur = _basePtr + _lCur;
                    _pEnd = _basePtr + _lEnd;
                    _chunkTable.Clear();

                    part.Includes = new(); //Initialize part
                    RefList[_lCur] = part.Name; //Add reference name

                    var isInit = false; // part.Type == PartType.Table;
                    var current = part.Struct ?? "Binary";
                    var locations = new List<Location>();
                    var chunks = new List<TableEntry>();
                    TableEntry last = null;
                    while (_pCur < _pEnd)
                    {
                        if (_chunkTable.TryGetValue(_lCur, out var value))
                        {
                            if (isInit) isInit = false;
                            current = value;

                        }
                        else if (isInit)
                        { locations.Add((Location)ParseType("Offset")); continue; }
                        else if (last != null)
                        {
                            if (last.Object is not List<object> list)
                                last.Object = list = [last.Object];
                            list.Add(ParseType(current));
                            continue;
                        }

                        chunks.Add(last = new(_lCur) { Object = ParseType(current) });
                    }

                    part.ObjectRoot = new TableGroup() { Locations = locations, Blocks = chunks };

                }

        }

        public void ResolveReferences()
        {
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
                {
                    _part = part;
                    ResolveObject(part.ObjectRoot);
                }
        }

        private void ResolveObject(object obj)
        {
            if (obj is string str)
            {
                for (var ix = str.IndexOf(RefChar); ix >= 0; ix = str.IndexOf(RefChar, ix + 7))
                {
                    var sLoc = Location.Parse(str.Substring(ix + 1, 6));
                    ResolveName(sLoc);
                    //str = str.Replace(str.Substring(ix, 7), ResolveName(part, sLoc));
                }
            }
            else if (obj is IEnumerable arr)
                foreach (var o in arr)
                    ResolveObject(o);
            else if (obj is Location loc)
                ResolveName(loc);
            else if (obj is StructDef sdef)
                ResolveObject(sdef.Parts);
            else if (obj is TableEntry tab)
                ResolveObject(tab.Object);
            else if (obj is TableGroup tgrp)
            {
                ResolveObject(tgrp.Locations);
                ResolveObject(tgrp.Blocks);
            }
            else if (obj is Op op)
            {
                for (var cur = op; op != null; op = op.Next)
                {
                    for (int i = 0; i < op.Operands.Length; i++)
                    {
                        var opnd = op.Operands[i];
                        if (opnd is Location l)
                        {
                            //op.Operands[i] = ResolveName(l);
                            //AddInclude(_part, l);
                            ResolveName(l);
                        }
                    }
                }
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
                {
                    _part = part;
                    _isInline = true;

                    if (inBlock) writer.WriteLine("------------------------------------"); //Serparator
                    else inBlock = true;

                    WriteObject(writer, part.ObjectRoot, 0);
                }
            }
        }

        private void WriteObject(StreamWriter writer, object obj, int depth)
        {
            void Indent()
            { for (int i = 0; i < depth; i++) writer.Write("  "); }

            if (!_isInline)
            {
                writer.WriteLine();
                Indent();
            }

            if (obj is TableGroup tGroup)
            {
                if (tGroup.Locations.Any())
                {
                    writer.Write($"{_part.Name} "); //Label
                    WriteObject(writer, tGroup.Locations, depth);
                    writer.WriteLine();
                }

                foreach (var t in tGroup.Blocks)
                {
                    _isInline = true;
                    writer.Write($"{(RefList.TryGetValue(t.Location, out var s) ? s : $"loc_{t.Location}")} ");
                    WriteObject(writer, t.Object, depth);
                    writer.WriteLine();
                    _isInline = false;
                }
                return;
            }


            if (obj is StructDef sDef)
            {
                writer.Write($"{sDef.Name} < ");
                _isInline = true;
                bool first = true;
                foreach (var o in sDef.Parts)
                {
                    if (first) first = false;
                    else writer.Write(", ");
                    WriteObject(writer, o, depth);
                }
                writer.Write(" >");
                _isInline = false;

            }
            else if (obj is Op op)
            {
                bool first = true;
                writer.WriteLine("{");
                _isInline = true;

                while (op != null) //Process each instruction in sequence
                {
                    if(first)
                    {
                        first = false;
                    }
                    else if (RefList.TryGetValue(op.Location, out var label)) //Check for code label
                    {
                        //if (first)
                        //{
                        //    //if (inBlock) writer.WriteLine("--------------------");
                        //    writer.WriteLine($"{label} {{"); //Write label
                        //    first = false;
                        //}
                        //else
                        //{
                            writer.WriteLine();
                            writer.WriteLine($"  {label}:"); //Write label
                        //}
                        //inBlock = true;
                    }

                    writer.Write($"    {op.Code.Mnem} ");
                    if (DbRoot.CopLib.Formats.TryGetValue(op.Code.Mode, out var format))
                    {
                        //We are relying on the fact that formats only have one operand
                        var o = op.Operands[0];
                        writer.Write(format, o is Location l ? ResolveName(l) : o);
                    }
                    else if (op.CopDef != null)
                    {
                        writer.Write($"[{op.CopDef.Mnem}]");
                        var len = op.Operands.Length;
                        if (len > 1)
                        {
                            for (int i = 1; i < len; i++)
                            {
                                writer.Write(i == 1 ? " ( " : ", ");
                                WriteObject(writer, op.Operands[i], depth + 1);
                            }
                            writer.Write(" )");
                        }
                    }
                    else
                    {

                    }
                    writer.WriteLine();
                    op = op.Next;
                }

                _isInline = false;
                writer.WriteLine("}");
            }
            else if (obj is Location l) writer.Write(ResolveName(l));
            else if (obj is byte b) writer.Write("#{0:X2}", b);
            else if (obj is ushort s) writer.Write("#{0:X4}", s);
            else if (obj is byte[] a)
            {
                writer.Write("#");
                writer.Write(Convert.ToHexString(a));
            }
            else if (obj is string[] sArr)
                foreach (var sa in sArr)
                    WriteObject(writer, sa, depth);
            else if (obj is string str)
            {
                for (var ix = str.IndexOf(RefChar); ix >= 0; ix = str.IndexOf(RefChar))
                {
                    var sLoc = Location.Parse(str.Substring(ix + 1, 6));
                    str = str.Replace(str.Substring(ix, 7), ResolveName(sLoc));
                }
                writer.Write('`');
                writer.Write(str);
                writer.Write('`');
            }
            else if (obj is IEnumerable arr)
            {
                writer.Write('[');
                _isInline = false;
                foreach (var o in arr)
                    WriteObject(writer, o, depth + 1);

                writer.WriteLine();
                Indent();
                writer.Write(']');
                _isInline = true;
            }
            else writer.Write(obj);

            if (depth == 0) writer.WriteLine();

        }
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
}
