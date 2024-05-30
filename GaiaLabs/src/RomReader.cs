using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GaiaLabs
{
    public unsafe class RomReader
    {
        const char RefChar = '~';

        public DbRoot DbRoot { get; set; }
        public Dictionary<Location, string> RefList = new();
        public Dictionary<Location, bool?> AccumulatorFlags = new();
        public Dictionary<Location, bool?> IndexFlags = new();
        public Dictionary<Location, byte> StackPosition = new();

        private byte* _basePtr;
        private byte* _pCur, _pEnd;
        private Location _lCur, _lEnd;
        private DbPart _part;
        private bool _isInline;
        private Dictionary<Location, string> _chunkTable = new();

        //private static void UpdateFlags<T>(IDictionary<Location, T?> dictionary, Location loc, T? value) where T : struct
        //{
        //    if (dictionary.TryGetValue(loc, out var entry)) //Look for existing value
        //    {
        //        if (entry == null)
        //            return; //Ignore entries with a hard unknown

        //        if (value != null && !value.Equals(entry)) //Check for disagreements
        //            return; //value = null; //Force hard unknown
        //        else if (value.Equals(entry))
        //            return; //Ignore when value will not change
        //    }

        //    dictionary[loc] = value;
        //}

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
            else //if (!name.StartsWith("loc_"))
                name = $"@{name}";

            return name;
        }

        //public void AddInclude(DbPart part, Location loc)
        //{
        //    if (part.Block.IsOutside(loc, out var p))
        //        part.Includes.Add(p);
        //}

        public DbRoot DumpDatabase(byte* basePtr, string outPath, string dbFile)
        {
            _basePtr = basePtr;
            DbRoot = DbRoot.FromFile(dbFile);


            ExtractFiles(outPath);

            //Process flag overrides
            foreach (var over in DbRoot.Overrides)
            {
                switch (over.Register)
                {
                    case RegisterType.M:
                        AccumulatorFlags[over.Location] = over.Value != 0u;
                        break;
                    case RegisterType.X:
                        IndexFlags[over.Location] = over.Value != 0u;
                        break;
                }
            }

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
                string folder = null, extension = "bin";

                switch (file.Type)
                {
                    case BinType.Bitmap: folder = "graphics"; break;
                    case BinType.Palette: folder = "palettes"; extension = "pal"; break;
                    case BinType.Music: folder = "music"; extension = "bgm"; break;
                    case BinType.Tileset: folder = "tilesets"; extension = "set"; break;
                    case BinType.Tilemap: folder = "tilemaps"; extension = "map"; break;
                    case BinType.Unknown: folder = "meta10"; break;
                    case BinType.Sound: folder = "sounds"; extension = "sfx"; break;
                };

                var filePath = outPath;
                if (folder != null)
                {
                    filePath = Path.Combine(outPath, folder);
                    Directory.CreateDirectory(filePath);
                }

                using var fileStream = File.Create(Path.Combine(filePath, $"{file.Name}.{extension}"));

                var pStart = _basePtr + file.Start;
                var len = (int)(file.End - file.Start);

                void copyBytes(byte* ptr, int len)
                {
                    if (file.Type == BinType.Palette)
                        for (ushort* pal = (ushort*)ptr, end = pal + len; pal < end; pal++)
                        {
                            var sample = *pal;
                            fileStream.WriteByte((byte)Math.Round((sample & 0x1F) * ImageConverter._sample5to8, 0));
                            fileStream.WriteByte((byte)Math.Round(((sample >> 5) & 0x1F) * ImageConverter._sample5to8, 0));
                            fileStream.WriteByte((byte)Math.Round(((sample >> 10) & 0x1F) * ImageConverter._sample5to8, 0));
                        }
                    else
                        for (byte* end = ptr + len; ptr < end; ptr++)
                            fileStream.WriteByte(*ptr);
                }

                if (file.Compressed)
                {
                    var data = Compression.Expand(pStart);
                    fixed (byte* dPtr = data)
                        copyBytes(dPtr, data.Length);
                }
                else
                    copyBytes(pStart, len);
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
                                if (r == cmd.Delimiter.Value) break;
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
                if (c == 0xCA || c == 0xC0)
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

        private Op ParseAsm(Registers reg)
        {
            //var ptr = rom + loc;
            var loc = _lCur;
            var opCode = *Advance();
            if (!Asm.OpCodes.TryGetValue(opCode, out var code))
                throw new("Unknown OpCode");

            List<object> operands = new(8);
            int size = code.Size;
            CopDef copDef = null;


            if (size == -2) //Handle variable-size operand
                if ((code.Code & 0xF) == 0x9) //Accumulator operations?
                    if (!(reg.AccumulatorFlag ?? false)) size = 3; //Check status of m flag
                    else size = 2;
                else if (!(reg.IndexFlag ?? false)) size = 3; //Check status of x flag
                else size = 2;

            var next = loc + (uint)size;

            Location xferRegs(Location loc)
            {
                if (reg.AccumulatorFlag != null)
                    AccumulatorFlags.TryAdd(loc, reg.AccumulatorFlag);
                if (reg.IndexFlag != null)
                    IndexFlags.TryAdd(loc, reg.IndexFlag);
                if (reg.Stack.Location > 0)
                    StackPosition.TryAdd(loc, (byte)reg.Stack.Location);

                if ((code.Mnem == "JSR" || code.Mnem == "JSL")
                    && DbRoot.Returns.TryGetValue(loc, out var over))
                {
                    switch (over.Register)
                    {
                        case RegisterType.M: reg.AccumulatorFlag = over.Value != 0u; break;
                        case RegisterType.X: reg.IndexFlag = over.Value != 0u; break;
                    }
                }

                return loc;
            }

            Location noteType(Location loc, string type, bool silent = false)
            {
                _chunkTable.TryAdd(loc, type);
                if (!silent && type == "Code")
                    return xferRegs(loc);
                return loc;
            }

            switch (code.Mode)
            {
                case AddressingMode.Implied:
                    switch (code.Mnem)
                    {
                        case "PHD": reg.Stack.Push(reg.Direct ?? 0); break;
                        case "PLD": reg.Direct = reg.Stack.PopUInt16(); break;
                        case "PHK": reg.Stack.Push(loc.Bank); break;
                        case "PHB": reg.Stack.Push(reg.DataBank ?? 0); break;
                        case "PLB": reg.DataBank = reg.Stack.PopByte(); break;
                        case "PHP": reg.Stack.Push((byte)reg.StatusFlags); break;
                        case "PLP": reg.StatusFlags = (StatusFlags)reg.Stack.PopByte(); break;

                        case "PHA":
                            if (reg.AccumulatorFlag == true)
                                reg.Stack.Push((byte)(reg.Accumulator ?? 0));
                            else
                                reg.Stack.Push(reg.Accumulator ?? 0);
                            break;

                        case "PLA":
                            if (reg.AccumulatorFlag == true)
                                reg.Accumulator = (ushort)((reg.Accumulator ?? 0) & 0xFF00u | reg.Stack.PopByte());
                            else
                                reg.Accumulator = reg.Stack.PopUInt16();
                            break;

                        case "PHX":
                            if (reg.IndexFlag == true)
                                reg.Stack.Push((byte)(reg.XIndex ?? 0));
                            else
                                reg.Stack.Push(reg.XIndex ?? 0);
                            break;

                        case "PLX":
                            if (reg.IndexFlag == true)
                                reg.XIndex = (ushort)((reg.XIndex ?? 0) & 0xFF00u | reg.Stack.PopByte());
                            else
                                reg.XIndex = reg.Stack.PopUInt16();
                            break;

                        case "PHY":
                            if (reg.IndexFlag == true)
                                reg.Stack.Push((byte)(reg.YIndex ?? 0));
                            else
                                reg.Stack.Push(reg.YIndex ?? 0);
                            break;

                        case "PLY":
                            if (reg.IndexFlag == true)
                                reg.YIndex = (ushort)((reg.YIndex ?? 0) & 0xFF00u | reg.Stack.PopByte());
                            else
                                reg.YIndex = reg.Stack.PopUInt16();
                            break;

                        case "XBA":
                            reg.Accumulator = (ushort)((reg.Accumulator ?? 0) >> 8 | (reg.Accumulator ?? 0) << 8);
                            break;
                    }
                    break;

                case AddressingMode.Immediate:
                    if (size == 3)
                        operands.Add(*(ushort*)Advance(2));
                    else
                        operands.Add(*Advance());

                    switch (code.Mnem)
                    {
                        case "LDA":
                            reg.Accumulator = size == 3 ? (ushort)operands[^1]
                                : (ushort)((reg.Accumulator ?? 0) & 0xFF00u | (byte)operands[^1]);
                            break;
                        case "LDX":
                            reg.XIndex = size == 3 ? (ushort)operands[^1]
                                : (ushort)((reg.XIndex ?? 0) & 0xFF00u | (byte)operands[^1]);
                            break;
                        case "LDY":
                            reg.YIndex = size == 3 ? (ushort)operands[^1]
                                : (ushort)((reg.YIndex ?? 0) & 0xFF00u | (byte)operands[^1]);
                            break;
                        case "SEP":
                        case "REP":
                            var flag = (StatusFlags)(byte)operands[^1];
                            if (flag.HasFlag(StatusFlags.AccumulatorMode))
                                AccumulatorFlags.TryAdd(_lCur, code.Mnem == "SEP");
                            if (flag.HasFlag(StatusFlags.IndexMode))
                                IndexFlags.TryAdd(_lCur, code.Mnem == "SEP");
                            break;
                    }



                    break;

                case AddressingMode.AbsoluteIndirect:
                case AddressingMode.AbsoluteIndirectLong:
                case AddressingMode.AbsoluteIndexedIndirect:
                    operands.Add(next & 0x3F0000u | (uint)*(ushort*)Advance(2));
                    break;

                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteIndexedX:
                case AddressingMode.AbsoluteIndexedY:
                    var refLoc = *(ushort*)Advance(2);
                    if (code.Mnem[0] == 'J')
                        operands.Add(noteType(next & 0x3F0000u | (uint)refLoc, "Code")); //Add PC bank
                    else if (code.Mnem[0] == 'P')
                        operands.Add(refLoc);
                    else
                        operands.Add(new Address(reg.DataBank ?? 0x81, refLoc)); //Add Data bank
                    break;

                case AddressingMode.AbsoluteLong:
                case AddressingMode.AbsoluteLongIndexedX:
                    operands.Add((Location)(*(ushort*)Advance(2) | ((uint)*Advance() << 16)));
                    if (code.Mnem[0] == 'J')
                        noteType((Location)operands[^1], "Code");
                    break;

                case AddressingMode.BlockMove:
                    operands.Add(*Advance());
                    operands.Add(*Advance());
                    break;

                case AddressingMode.DirectPage:
                case AddressingMode.DirectPageIndexedIndirectX:
                case AddressingMode.DirectPageIndexedX:
                case AddressingMode.DirectPageIndexedY:
                case AddressingMode.DirectPageIndirect:
                case AddressingMode.DirectPageIndirectIndexedY:
                case AddressingMode.DirectPageIndirectLong:
                case AddressingMode.DirectPageIndirectLongIndexedY:
                    operands.Add(new Address(0, (ushort)((reg.Direct ?? 0) + *Advance())));
                    break;

                case AddressingMode.PCRelative:
                    Location relative = (uint)((int)next.Offset + *(sbyte*)Advance());
                    goto noteRelative;

                case AddressingMode.PCRelativeLong:
                    relative = (uint)((int)next.Offset + *(short*)Advance(2));
                noteRelative:
                    operands.Add(code.Mnem == "BRA" || code.Mnem == "BRL"
                        ? noteType(relative, "Code")
                        : xferRegs(relative));
                    break;

                case AddressingMode.StackRelative:
                case AddressingMode.StackRelativeIndirectIndexedY:
                    operands.Add(*Advance());
                    break;

                case AddressingMode.StackInterrupt:
                    var cmd = *Advance();
                    operands.Add(cmd);
                    if (code.Mnem == "COP")
                    {
                        if (!DbRoot.CopLib.Codes.TryGetValue(cmd, out copDef))
                            throw new("Unknown COP command");

                        //var cop = db.CopLib.Codes[cmd];
                        //var parts = cop.Parts;
                        //size = cop.Size + 2;
                        foreach (var p in copDef.Parts)
                        {
                            var str = p;
                            bool isPtr = str[0] == '*', isAddr = str[0] == '&';
                            var otherStr = (isPtr || isAddr) ? str[1..] : "Binary";

                            if (isPtr) str = "Offset";
                            else if (isAddr) str = "Address";

                            if (!Enum.TryParse<MemberType>(str, true, out var mtype))
                                throw new("Cannot use structs in cop def");

                            Location copLoc(Location loc)
                            {
                                if (p != "Address")
                                {
                                    if (!isPtr && !isAddr)
                                        throw new("Should not note non-pointer types");
                                    return noteType(loc, otherStr, true);
                                }
                                return loc;
                            }

                            switch (mtype)
                            {
                                case MemberType.Byte: operands.Add(*Advance()); break;
                                case MemberType.Word: operands.Add(*(ushort*)Advance(2)); break;
                                case MemberType.Offset: operands.Add(copLoc(*(ushort*)Advance(2) | (next.Offset & 0x3F0000u))); break;
                                case MemberType.Address: operands.Add(copLoc(*(ushort*)Advance(2) | ((uint)*Advance() << 16))); break;
                                default: throw new("Unsuported COP member type");
                            }
                        }
                    }
                    break;
            }

            return new Op { Location = loc, Code = code, Size = (byte)(_lCur - loc), Operands = [.. operands], CopDef = copDef };
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
                if (StackPosition.TryGetValue(_lCur, out var stack))
                    reg.Stack.Location = stack;

                var op = ParseAsm(reg); //Parse instruction

                //if (op.Code.Mnem == "SEP")
                //{
                //    var flag = (StatusFlags)op.Operands[0];
                //    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                //        UpdateFlags(AccumulatorFlags, _lCur, true);
                //    if (flag.HasFlag(StatusFlags.IndexMode))
                //        UpdateFlags(IndexFlags, _lCur, true);
                //}
                //else if (op.Code.Mnem == "REP")
                //{
                //    var flag = (StatusFlags)op.Operands[0];
                //    if (flag.HasFlag(StatusFlags.AccumulatorMode))
                //        UpdateFlags(AccumulatorFlags, _lCur, false);
                //    if (flag.HasFlag(StatusFlags.IndexMode))
                //        UpdateFlags(IndexFlags, _lCur, false);
                //}

                //for (var i = 0; i < op.Operands.Length; i++)
                //{
                //    var obj = op.Operands[i];
                //    if (obj is Location r)
                //    {
                //        //if (_part.IsInside(r))
                //        //{
                //        if (op.CopDef != null)
                //        {
                //            var type = op.CopDef.Parts[i - 1];
                //            if (type != "Address")
                //            {
                //                if (type[0] == '*' || type[0] == '&')
                //                    type = type[1..];
                //                else
                //                    throw new("Should not attach non-pointer types");
                //                _chunkTable.TryAdd(r, type);
                //            }
                //        }
                //        else if (op.Code.Mnem[0] == 'J' &&
                //            (op.Code.Mode == AddressingMode.Absolute || op.Code.Mode == AddressingMode.AbsoluteLong))
                //        {
                //            _chunkTable.TryAdd(r, "Code");
                //        }
                //        //}

                //        if (reg.AccumulatorFlag != null)
                //            UpdateFlags(AccumulatorFlags, r, reg.AccumulatorFlag);
                //        if (reg.IndexFlag != null)
                //            UpdateFlags(IndexFlags, r, reg.IndexFlag);
                //    }
                //}

                if (prev == null)
                    head = op; //Set head
                else
                    op.Prev = prev; //Set prev

                //Advance(op.Size); //Advance location
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
                    //_chunkTable.Clear();

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
                    if (first)
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
                        if (op.Operands[0] is Location l)
                            writer.Write(format, ResolveName(l));
                        else
                            writer.Write(format, op.Operands);
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
