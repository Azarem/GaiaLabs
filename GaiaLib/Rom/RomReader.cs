using GaiaLib.Asm;
using GaiaLib.Database;
using System.Collections;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GaiaLib.Rom
{
    public unsafe partial class RomReader : IDisposable
    {
        public static readonly char[] RefChar = ['~', '^'];
        public const float Sample5to8 = 255.3f / 31f;

        public DbRoot DbRoot;
        public Dictionary<Location, string> RefList = new();
        public Dictionary<Location, bool?> AccumulatorFlags = new();
        public Dictionary<Location, bool?> IndexFlags = new();
        public Dictionary<Location, byte?> BankNotes = new();
        public Dictionary<Location, byte> StackPosition = new();

        private byte* _basePtr;
        private byte* _pCur, _pEnd;
        private Location _lCur, _lEnd;
        private DbPart? _part;
        private bool _isInline;
        private Dictionary<Location, string> _chunkTable = new();
        private Dictionary<Location, int> _markerTable = new();

        private MemoryMappedFile? _mappedFile;
        private MemoryMappedViewAccessor? _viewAccessor;

        public RomReader(string romPath, string dbPath)
        {
            DbRoot = DbRoot.FromFile(dbPath);
            _mappedFile = MemoryMappedFile.CreateFromFile(romPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _viewAccessor = _mappedFile.CreateViewAccessor(0, new FileInfo(romPath).Length, MemoryMappedFileAccess.Read);
            _viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref _basePtr);
        }

        public void Dispose()
        {
            if (_viewAccessor != null)
            {
                _viewAccessor.Dispose();
                _viewAccessor = null;
            }
            if (_mappedFile != null)
            {
                _mappedFile.Dispose();
                _mappedFile = null;
            }
            GC.SuppressFinalize(this);
        }

        public void DumpDatabase(string outPath)
        {
            ExtractFiles(outPath);
            ExtractSfx(outPath);

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
                    case RegisterType.B:
                        BankNotes[over.Location] = (byte)over.Value.Value;
                        break;
                }
            }

            AnalyzeBlocks();

            ResolveReferences();
            WriteBlocks(outPath);
        }

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

        private void ResolveInclude(Location loc, bool isBranch)
        {
            if (_part.Block.IsOutside(loc, out var p) && p != null)
                _part.Includes.Add(p);
            else if (isBranch && !RefList.ContainsKey(loc))
                RefList[loc] = $"loc_{loc}";
        }

        private void ResolveMnemonic(Address addr)
        {
            HexString hexString = (addr.Bank == 0x7E || addr.Bank == 0x7F) ? new((uint)addr) : new(addr.Offset);

            if (DbRoot.Mnemonics.TryGetValue(hexString, out var label))
            {
                uint opnd = 0;
                char op = (char)0;
                var ix = label.IndexOfAny(Process._operators);

                if (ix >= 0)
                {
                    op = label[ix];
                    opnd = uint.Parse(label[(ix + 1)..], NumberStyles.HexNumber);
                    label = label[..ix];

                    switch (op)
                    {
                        case '-': hexString.Value += opnd; break;
                        case '+': hexString.Value -= opnd; break;
                        default: throw new ArgumentException();
                    }
                }

                _part.Block.Mnemonics.Add((hexString, label));
            }
        }

        private string ResolveName(Location loc, AddressType type, bool isBranch)
        {
            var prefix = Address.CodeFromType(type);

            string? name, label = null;

            if (DbRoot.Rewrites.TryGetValue(loc, out var refLoc))
            {
                //var offIx = label.IndexOfAny(['-', '+']);
                //if (offIx < 0) offIx = label.Length;
                //loc = Location.Parse(label[..offIx]);
                //label = label[offIx..];
                var offset = (int)loc.Offset - (int)refLoc.Offset;
                char cmd;

                if (offset < 0)
                {
                    offset = -offset;
                    cmd = '-';
                }
                else
                    cmd = '+';

                if (_chunkTable.TryGetValue(refLoc, out var ctype) && ctype == "WideString")
                {
                    _markerTable[refLoc] = offset;
                    _markerTable[loc] = offset;
                    label = $"{cmd}M";
                }
                else
                    label = $"{cmd}{offset:X}";

                loc = refLoc;
            }


            //if (strictAsm.Contains(_part.Block.Name) && _part.Block.IsOutside(loc, out var p))
            //{
            //    name = (loc.Offset | 0x800000u).ToString("X6");
            //    prefix = '$';
            //}
            //else 
            if (!RefList.TryGetValue(loc, out name))
            {
                if (isBranch)
                {
                    name = $"loc_{loc}";
                    RefList[loc] = name;
                }
                else
                {
                    uint closest = 0x1A0;
                    string? bestMatch = null;
                    Location? bestLoc = null;
                    foreach (var entry in RefList)
                    {
                        if (entry.Key > loc)
                            continue;

                        var range = loc.Offset - entry.Key.Offset;
                        if (range >= closest)
                            continue;

                        closest = range;
                        bestMatch = entry.Value;
                        bestLoc = entry.Key;

                        if (closest == 1)
                            break;
                    }

                    if (bestMatch != null)
                    {
                        if (_chunkTable.TryGetValue(bestLoc.Value, out var ctype) && ctype == "WideString")
                        {
                            _markerTable[bestLoc.Value] = (int)closest;
                            _markerTable[loc] = (int)closest;
                            name = $"{bestMatch}+M";
                        }
                        else
                            name = $"{bestMatch}+{closest:X}";
                        goto Next;
                    }

                    //File references
                    var fileMatch = _part.Block.Root.Files.FirstOrDefault(x => x.Start <= loc && x.End > loc);
                    if (fileMatch != null)
                        name = $"{fileMatch.Name}+{loc.Offset - fileMatch.Start.Offset:X}";
                    else
                        name = loc.ToString();
                }
            }


        Next:
            if (prefix != null)
                name = $"{prefix}{name}";

            if (!string.IsNullOrEmpty(label))
                name += label;

            return name;
        }

        //public void AddInclude(DbPart part, Location loc)
        //{
        //    if (part.Block.IsOutside(loc, out var p))
        //        part.Includes.Add(p);
        //}

        private void ExtractFiles(string outPath)
        {
            foreach (var file in DbRoot.Files)
            {
                var start = file.Start;
                RefList[start] = file.Name;
                //string folder = "misc", extension = "bin";
                ushort mapSample = 0;
                uint metaSample = 0;

                var res = DbRoot.GetPath(file.Type);


                //switch (file.Type)
                //{
                //    case BinType.Bitmap: folder = "graphics"; break;
                //    case BinType.Palette: folder = "palettes"; extension = "pal"; break;
                //    case BinType.Music: folder = "music"; extension = "bgm"; break;
                //    case BinType.Tileset: folder = "tilesets"; extension = "set"; break;
                //    case BinType.Tilemap:
                //        folder = "tilemaps";
                //        extension = "map";
                //        mapSample = *(ushort*)(_basePtr + start);
                //        start += 2;
                //        break;
                //    case BinType.Meta17:
                //        metaSample = *(uint*)(_basePtr + start);
                //        start += 4;
                //        break;
                //    case BinType.Spritemap: folder = "spritemaps"; break;
                //    case BinType.Assembly:
                //        folder = "asm"; extension = "asm";
                //        continue;
                //        //case BinType.Sound: folder = "sfx"; extension = "bin"; break;
                //};

                var filePath = outPath;
                if (!string.IsNullOrWhiteSpace(res.Folder))
                {
                    filePath = Path.Combine(outPath, res.Folder);
                    Directory.CreateDirectory(filePath);
                }

                filePath = Path.Combine(filePath, $"{file.Name}.{res.Extension}");
                if (File.Exists(filePath))
                    continue;


                if (file.Type == BinType.Tilemap)
                {
                    mapSample = *(ushort*)(_basePtr + start);
                    start += 2;
                }
                else if (file.Type == BinType.Meta17)
                {
                    metaSample = *(uint*)(_basePtr + start);
                    start += 4;
                }

                using var fileStream = File.Create(Path.Combine(filePath, $"{file.Name}.{res.Extension}"));

                var pStart = _basePtr + start;
                var len = (int)(file.End - start);

                void copyBytes(byte* ptr, int len)
                {
                    //if (file.Type == BinType.Palette)
                    //    for (ushort* pal = (ushort*)ptr, end = pal + len; pal < end; pal++)
                    //    {
                    //        var sample = *pal;
                    //        fileStream.WriteByte((byte)Math.Round((sample & 0x1F) * Sample5to8, 0));
                    //        fileStream.WriteByte((byte)Math.Round(((sample >> 5) & 0x1F) * Sample5to8, 0));
                    //        fileStream.WriteByte((byte)Math.Round(((sample >> 10) & 0x1F) * Sample5to8, 0));
                    //    }
                    //else
                    {
                        if (file.Type == BinType.Tilemap)
                        {
                            fileStream.WriteByte((byte)mapSample);
                            fileStream.WriteByte((byte)(mapSample >> 8));
                        }
                        else if (file.Type == BinType.Meta17)
                        {
                            fileStream.WriteByte((byte)metaSample);
                            fileStream.WriteByte((byte)(metaSample >> 8));
                            fileStream.WriteByte((byte)(metaSample >> 16));
                            fileStream.WriteByte((byte)(metaSample >> 24));
                        }

                        for (byte* end = ptr + len; ptr < end; ptr++)
                            fileStream.WriteByte(*ptr);
                    }
                }

                if (file.Compressed == true)
                {
                    var data = Compression.Expand(pStart, len);
                    fixed (byte* dPtr = data)
                        copyBytes(dPtr, data.Length);
                }
                else
                {
                    if (file.Compressed == false) //Skip header for uncompressed bitmaps
                    {
                        pStart += 2;
                        len -= 2;
                    }
                    copyBytes(pStart, len);
                }
            }
        }

        private void ExtractSfx(string outPath)
        {
            var res = DbRoot.GetPath(BinType.Sound);
            //string folder = "sfx", extension = "bin";
            string folderPath = Path.Combine(outPath, res.Folder);
            Directory.CreateDirectory(folderPath);

            var ptr = _basePtr + 0x50000u;
            var offset = 0;

            byte readByte()
            {
                if (offset == 0x8000u)
                {
                    ptr += 0x10000;
                    offset = 0;
                }
                return ptr[offset++];
            }

            for (int i = 0; i < 0x3C; i++)
            {
                int size = readByte() | (readByte() << 8);

                string filePath = Path.Combine(folderPath, $"sfx{i:X2}.{res.Extension}");
                if (File.Exists(filePath))
                    for (int x = 0; x < size; x++)
                        readByte();
                else
                    using (var file = File.Create(filePath))
                        for (int x = 0; x < size; x++)
                            file.WriteByte(readByte());
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
            if (_pCur >= _pEnd /*|| _lCur >= _lEnd*/)
                return false;
            if (_chunkTable.ContainsKey(_lCur))
                return false;
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
                            builder.Append($"^{loc:X6}");
                            break;

                        case MemberType.Address:
                            loc = *(ushort*)Advance(2) | ((uint)*Advance() << 16);
                            builder.Append($"~{loc:X6}");
                            break;

                        case MemberType.Binary:
                            bool sfirst = true;
                            do
                            {
                                var r = *Advance();
                                if (r == cmd.Delimiter.Value.Value) break;
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

        private StringWrapper ParseASCIIString()
        {
            var dict = DbRoot.StringCommands;
            var builder = new StringBuilder();
            var strLoc = _lCur;

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
            return new(builder.ToString(), StringType.ASCII, strLoc);
        }

        private StringWrapper ParseCharString()
        {
            var builder = new StringBuilder();
            var dict = DbRoot.WideCommands;
            var strLoc = _lCur;

            do
            {
                var c = *Advance();
                if (c == 0xCA)
                    break;

                if (dict.TryGetValue(c, out var cmd))
                    ResolveCommand(cmd, builder);
                else
                {
                    var index = (c & 0x70) >> 1 | (c & 0x07);
                    builder.Append(DbRoot.CharMap[index]);
                }
            } while (CanContinue());

            return new(builder.ToString(), StringType.Char, strLoc);
        }

        private StringWrapper ParseWideString()
        {
            var builder = new StringBuilder();
            var dict = DbRoot.WideCommands;
            var strLoc = _lCur;

            do
            {
                var c = *Advance();
                if (c == 0xCA)
                {
                    if (*_pCur == 0xCA)
                    {
                        Advance();
                        builder.Append($"[{dict[0xCA].Value}]");
                    }
                    break;
                }

                if (dict.TryGetValue(c, out var cmd))
                    ResolveCommand(cmd, builder);
                else
                {
                    var index = (c & 0xE0) >> 1 | (c & 0x0F);
                    builder.Append(DbRoot.WideMap[index]);
                }
            } while (CanContinue());

            return new(builder.ToString(), StringType.Wide, strLoc);
        }

        private Op ParseAsm(Registers reg)
        {
            //var ptr = rom + loc;
            var loc = _lCur;
            var opCode = *Advance();
            if (!OpCode.All.TryGetValue(opCode, out var code))
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

            string noteType(Location loc, string type, bool silent = false)
            {
                _chunkTable.TryAdd(loc, type);

                if (!RefList.TryGetValue(loc, out var name))
                {
                    name = type.ToLower();
                    while (_adrSpace.Contains(name[0]))
                        name = name[1..] + "_list";

                    name = $"{name}_{loc}";
                    RefList[loc] = name;
                }

                if (!silent && type == "Code")
                    xferRegs(loc);

                return name;
            }

            if (code.Mnem == "LDA") reg.Accumulator = null;
            else if (code.Mnem == "LDX") reg.XIndex = null;
            else if (code.Mnem == "LDY") reg.YIndex = null;

            byte bank;
            byte? xBank1 = null, xBank2 = null;
            string? xForm1 = null, xForm2 = null;

            (xBank1, xForm1) = getXform();

            (byte?, string?) getXform()
            {
                byte? b = null;
                if (DbRoot.Transforms.TryGetValue(_lCur, out string? xf) && xf != "")
                {
                    var match = BankRegex().Match(xf);
                    if (match.Success)
                        b = byte.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
                    else
                    {
                        var str = xf.TrimStart(Process._addressspace);
                        b = RefList.FirstOrDefault(x => x.Value == str).Key.Bank;
                    }
                }

                return (b, xf);
            }

            //byte? xformBank = null;
            //if (DbRoot.Transforms.TryGetValue(_lCur, out var xform) && xform != "")
            //{
            //    var match = BankRegex().Match(xform);
            //    if (match.Success)
            //        xformBank = byte.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber);
            //    else
            //    {
            //        var str = xform.TrimStart(Process._addressspace);
            //        xformBank = RefList.First(x => x.Value == str).Key.Bank;
            //    }
            //}

            switch (code.Mode)
            {
                case AddressingMode.Stack:
                case AddressingMode.Implied:
                    switch (code.Mnem)
                    {
                        case "PHD": reg.Stack.Push(reg.Direct ?? 0); break;
                        case "PLD": reg.Direct = reg.Stack.PopUInt16(); break;
                        case "PHK": reg.Stack.Push((byte)(loc.Bank | 0x80)); break;
                        case "PHB": reg.Stack.Push(reg.DataBank ?? 0x81); break;
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
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteIndexedX:
                case AddressingMode.AbsoluteIndexedY:
                    var refLoc = *(ushort*)Advance(2);
                    //    operands.Add(refLoc);
                    //else
                    {
                        var isPush = code.Mnem[0] == 'P';
                        if (isPush)
                            refLoc++;
                        var isJump = isPush || code.Mnem[0] == 'J';
                        bank = xBank1 ?? // xform?.Bank != null ? (byte)xform.Bank.Value :
                            (isJump ? (byte)(next >> 16) : (reg.DataBank ?? 0x81));

                        var addr = new Address(bank, refLoc);
                        if (addr.Space == AddressSpace.ROM)
                        {
                            var wrapper = new LocationWrapper((Location)addr, AddressType.Offset);
                            if (isJump)
                            {
                                var name = noteType(wrapper.Location, "Code", isPush); //Add PC bank

                                if (isPush)
                                {
                                    operands.Add($"&{name}-1");
                                    break;
                                }
                            }
                            operands.Add(wrapper);
                        }
                        else
                            operands.Add(addr);
                    }
                    break;

                case AddressingMode.AbsoluteLong:
                case AddressingMode.AbsoluteLongIndexedX:
                    refLoc = *(ushort*)Advance(2);
                    bank = *Advance();
                    var adrs = new Address(bank, refLoc);
                    //if (bank == 0 || bank == 0x7E || bank == 0x7F || ((bank & 0x40) == 0 && refLoc < 0x8000))
                    //    operands.Add(new Address(bank, refLoc));
                    if (adrs.Space == AddressSpace.ROM)
                    {
                        var wrapper = new LocationWrapper((Location)adrs, AddressType.Address);
                        if (code.Mnem[0] == 'J')
                            noteType(wrapper.Location, "Code");
                        operands.Add(wrapper);
                    }
                    else
                        operands.Add(adrs);
                    break;

                case AddressingMode.BlockMove:
                    operands.Add(*Advance());
                    (xBank2, xForm2) = getXform();
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
                    operands.Add(*Advance());
                    //operands.Add(new Address(0, (ushort)((reg.Direct ?? 0) + *Advance())));
                    break;

                case AddressingMode.PCRelative:
                    Location relative = (uint)((int)next.Offset + *(sbyte*)Advance());
                    goto noteRelative;

                case AddressingMode.PCRelativeLong:
                    relative = (uint)((int)next.Offset + *(short*)Advance(2));
                noteRelative:
                    noteType(relative, "Code");
                    operands.Add(relative);
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
                            var isPtr = _adrSpace.Contains(p[0]);
                            string otherStr, str;
                            char? type;

                            (otherStr, type, str) = isPtr
                                ? (p[1..], p[0], p[0] == '&' ? "Offset" : "Address")
                                : (_part.Struct ?? "Binary", (char?)null, p);

                            //bool isPtr = str[0] == '*', isAddr = str[0] == '&';
                            //var otherStr = (isPtr || isAddr) ? str[1..] : "Binary";

                            //if (isPtr) str = "Offset";
                            //else if (isAddr) str = "Address";

                            if (!Enum.TryParse<MemberType>(str, true, out var mtype))
                                throw new("Cannot use structs in cop def");

                            DbRoot.Transforms.TryGetValue(_lCur, out var xform);

                            object copLoc(ushort offset, byte? bank)
                            {
                                if (!string.IsNullOrEmpty(xform))
                                    return xform;

                                if (bank == null && offset == 0)
                                    return offset;

                                var addr = new Address(bank ?? next.Bank, offset);
                                if (addr.Space == AddressSpace.ROM)
                                {
                                    var l = (Location)addr;
                                    if (p != "Address" && isPtr && !DbRoot.Rewrites.ContainsKey(l))
                                        noteType(l, otherStr, true);
                                    return new LocationWrapper(l, type != null ? Address.TypeFromCode(type.Value)
                                        : bank == null ? AddressType.Offset : AddressType.Address);
                                }
                                return addr;
                            }

                            object resolve(object obj)
                            {
                                if (!string.IsNullOrEmpty(xform))
                                    return xform;
                                return obj;
                            }

                            switch (mtype)
                            {
                                case MemberType.Byte: operands.Add(resolve(*Advance())); break;
                                case MemberType.Word: operands.Add(resolve(*(ushort*)Advance(2))); break;
                                case MemberType.Offset: operands.Add(copLoc(*(ushort*)Advance(2), null)); break;
                                case MemberType.Address: operands.Add(copLoc(*(ushort*)Advance(2), *Advance())); break;
                                default: throw new("Unsuported COP member type");
                            }
                        }
                    }
                    break;
            }

            void doXForm(string? xform, byte? bank, int oix)
            {
                if (xform == null)
                    return;

                if (xform == "")
                {
                    Location value = (next.Offset & 0xFF0000) | (ushort)operands[oix];
                    if (!RefList.TryGetValue(value, out xform))
                        RefList[value] = xform = $"loc_{value}";
                    operands[oix] = $"&{xform}";
                }
                else
                {
                    var op = operands[oix];

                    if (op is ushort us)
                        op = (Location)((uint)bank.Value << 16 | us);
                    else if (op is LocationWrapper lw)
                        op = lw.Location;

                    if (op is Location l)
                        ResolveInclude(l, false);

                    operands[oix] = xform;
                }
            }

            doXForm(xForm1, xBank1, 0);
            doXForm(xForm2, xBank2, 1);

            //if (xForm1 != null)// && (xform.Type == null || xform.Type == "*" || xform.Type == "^"))
            //{
            //    if (xForm1 == "")
            //    {
            //        Location value = (next.Offset & 0xFF0000) | (ushort)operands[0];
            //        if (!RefList.TryGetValue(value, out xform))
            //            RefList[value] = xform = $"loc_{value}";
            //        operands[0] = $"&{xform}";
            //    }
            //    else
            //    {
            //        var op = operands[0];

            //        if (op is ushort us)
            //            op = (Location)((uint)xformBank.Value << 16 | us);
            //        else if (op is LocationWrapper lw)
            //            op = lw.Location;

            //        if (op is Location l)
            //            ResolveInclude(l, false);

            //        operands[0] = xform;
            //    }
            //    ////if(code.Mnem == "PEA")
            //    //if (xform.Type == null)
            //    //{
            //    //    var value = (ushort)operands[0];
            //    //    var addr = new Address((byte)(xform.Bank?.Value ?? reg.DataBank ?? 0x81u), value);
            //    //    operands[0] = new LocationWrapper((Location)addr, AddressType.Offset);
            //    //}
            //    //else
            //    //{
            //    //    //Ignore operand
            //    //    var entry = RefList.First(x => xform.Name.Equals(x.Value, StringComparison.CurrentCultureIgnoreCase));
            //    //    operands[0] = new LocationWrapper(entry.Key, xform.Type == "^" ? AddressType.Bank : AddressType.DBank);
            //    //}
            //}

            return new Op { Location = loc, Code = code, Size = (byte)(_lCur - loc), Operands = [.. operands], CopDef = copDef };
        }

        private List<Op> ParseCode(Registers reg)
        {
            var opList = new List<Op>();
            //var reg = new Registers();
            //Op prev = null, head = null;
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
                if (BankNotes.TryGetValue(_lCur, out var bnk))
                    reg.DataBank = bnk;
                if (StackPosition.TryGetValue(_lCur, out var stack))
                    reg.Stack.Location = stack;

                var op = ParseAsm(reg); //Parse instruction

                opList.Add(op);
            }

            return opList;
        }

        private object ParseLocation(ushort offset, byte? bank, string otherStr, char? cmd)
        {
            if (bank == null && offset == 0)
                return offset;

            var adrs = new Address(bank ?? (byte)(_lCur.Bank | (cmd == '@' ? 0xC0 : 0x80)), offset);
            if (adrs.Space == AddressSpace.ROM)
            {
                var loc = (Location)adrs;

                if (_part.Block.IsInside(loc, out var part) && !DbRoot.Rewrites.ContainsKey(loc))
                {
                    _chunkTable.TryAdd(loc, otherStr);
                    RefList.TryAdd(loc, $"{otherStr.ToLower()}_{loc}");
                }

                return new LocationWrapper(loc, cmd != null ? Address.TypeFromCode(cmd.Value)
                    : bank == null ? AddressType.Offset : AddressType.Address);
            }

            return adrs;
        }

        private static char[] _adrSpace = ['&', '@'];

        private object ParseType(string str, Registers reg, int depth, byte? bank = null)
        {
            var isPtr = _adrSpace.Contains(str[0]);
            string otherStr;
            char? cmd;

            (otherStr, cmd, str) = isPtr
                ? (str[1..], str[0], str[0] == '&' ? "Offset" : "Address")
                : (_part.Struct ?? "Binary", (char?)null, str);


            //bool isPtr = str[0] == '*', isAddr = str[0] == '&';
            //var otherStr = isPtr ? str[1..] : (_part.Struct ?? "Binary");

            //char cmd = isPtr ? str[0] : (char)0;
            //if (isPtr)
            //    str = str[0] == '&' ? "Offset" : "Address";

            //Parse raw values
            if (Enum.TryParse<MemberType>(str, true, out var mType))
                return mType switch
                {
                    MemberType.Byte => *Advance(),
                    MemberType.Word => _chunkTable.ContainsKey(_lCur + 1) ? (object)*Advance() : *(ushort*)Advance(2),
                    MemberType.Offset => ParseLocation(*(ushort*)Advance(2), bank, otherStr, cmd),
                    MemberType.Address => ParseLocation(*(ushort*)Advance(2), *Advance(), otherStr, cmd),
                    MemberType.Binary => ParseBinary(),
                    MemberType.String => ParseASCIIString(),
                    MemberType.CharString => ParseCharString(),
                    MemberType.WideString => ParseWideString(),
                    MemberType.Code => ParseCode(reg),
                    _ => throw new("Invalid member type"),
                };

            var parent = DbRoot.Structs[str];
            var delimiter = parent.Delimiter;
            var descriminator = parent.Descriminator;
            var objects = new List<object>();

            //Continue to iterate until end or delimiter is reached
            bool delReached;
            while (!(delReached = DelimiterReached(delimiter)))
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
                if (types != null)
                {
                    var members = types.Length;
                    var prevPtr = _pCur;
                    var parts = new object[members]; //Create new member collection
                    var def = new StructDef { Name = target.Name, Parts = parts };

                    //Parse each member of the struct
                    for (int i = 0; i < members; i++)
                        parts[i] = ParseType(types[i], null, depth + 1);

                    //Advance for descriminator when we have reached it
                    if (descriminator != null && descriminator.Value.Value == (uint)(_pCur - prevPtr))
                        Advance();

                    objects.Add(def);
                }

                if (!CanContinue()) break;
            }

            if (delReached && depth == 0)
                _chunkTable.TryAdd(_lCur, str);

            return objects;
        }

        private void AnalyzeBlocks()
        {
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
                {
                    _chunkTable[part.Start] = part.Struct;
                    RefList[part.Start] = part.Name;
                }

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
                    //RefList[_lCur] = part.Name; //Add reference name


                    //var isInit = false; // part.Type == PartType.Table;
                    var current = part.Struct ?? "Binary";
                    //var locations = new List<Location>();
                    var chunks = new List<TableEntry>();
                    var reg = new Registers();
                    byte? bank = part.Bank != null ? (byte)part.Bank.Value.Value : null;
                    TableEntry? last = null;
                    while (_pCur < _pEnd)
                    {
                        if (_chunkTable.TryGetValue(_lCur, out var value))
                        {
                            //if (isInit) isInit = false;
                            current = value;

                        }
                        //else if (isInit)
                        //{ locations.Add((Location)ParseType("Offset", null)); continue; }
                        else if (last != null)
                        {
                            if (last.Object is not List<object> list)
                                last.Object = list = [last.Object];
                            list.Add(ParseType(current, reg, 0, bank));
                            continue;
                        }

                        object wrapType()
                        {
                            var res = ParseType(current, reg, 0, bank);
                            if (_adrSpace.Contains(current[0]) && res is not List<object>)
                                res = new List<object>() { res };
                            return res;
                        }

                        chunks.Add(last = new(_lCur) { Object = wrapType() });
                    }

                    part.ObjectRoot = chunks;
                }

        }

        private void ResolveReferences()
        {
            foreach (var block in DbRoot.Blocks)
                foreach (var part in block.Parts)
                {
                    _part = part;
                    ResolveObject(part.ObjectRoot, false);
                }
        }

        private void ResolveObject(object obj, bool isBranch)
        {
            if (obj is string strn)
            {
            }
            else if (obj is IEnumerable arr)
                foreach (var o in arr)
                    ResolveObject(o, isBranch);
            else if (obj is Address addr)
                ResolveMnemonic(addr);
            else if (obj is Location loc)
                ResolveInclude(loc, isBranch);
            else if (obj is LocationWrapper lw)
                ResolveInclude(lw.Location, isBranch);
            else if (obj is StringWrapper sw)
            {
                var str = sw.String;
                for (var ix = str.IndexOfAny(RefChar); ix >= 0; ix = str.IndexOfAny(RefChar, ix + 7))
                {
                    var sloc = uint.Parse(str.Substring(ix + 1, 6), NumberStyles.HexNumber);
                    var addrs = new Address((byte)(sloc >> 16), (ushort)sloc);
                    if (addrs.Space == AddressSpace.ROM)
                    {
                        ResolveInclude(sloc, false);
                        var name = ResolveName(sloc, AddressType.Unknown, false);
                        var opix = name.IndexOfAny(Process._operators);
                        if (opix > 0)
                        {
                            var offset = name[opix + 1] == 'M'
                                ? _markerTable[sloc]
                                : int.Parse(name[(opix + 1)..], NumberStyles.HexNumber);

                            if (name[opix] == '-')
                                offset = -offset;

                            name = name[..opix];
                            var target = (uint)(sloc - offset);
                            _part.Block.IsOutside(sloc, out var prt);
                            if (prt != null)
                            {
                                //_part.Block.IsInside(sloc, out prt);
                                var root = prt.ObjectRoot as IEnumerable<TableEntry>;
                                var entry = root.First(x => x.Location == target).Object as StringWrapper;
                                entry.Marker = offset;

                                //sw.String = str = str[..(ix + 1)] + target.ToString("X6") + str[(ix + 7)..];
                            }

                        }
                    }
                    //var sLoc = Location.Parse(str.Substring(ix + 1, 6));
                    //str = str.Replace(str.Substring(ix, 7), ResolveName(part, sLoc));
                }
                //ResolveObject(sw.String, isBranch);
            }
            else if (obj is StructDef sdef)
                ResolveObject(sdef.Parts, isBranch);
            else if (obj is TableEntry tab)
                ResolveObject(tab.Object, isBranch);
            //else if (obj is TableGroup tgrp)
            //{
            //    ResolveObject(tgrp.Locations);
            //    ResolveObject(tgrp.Blocks);
            //}
            else if (obj is Op op)
            {
                var branch = op.Code.Mode == AddressingMode.PCRelative
                        || op.Code.Mode == AddressingMode.PCRelativeLong
                        || op.Code.Mnem[0] == 'J';
                foreach (var opnd in op.Operands)
                    ResolveObject(opnd, branch);
                //for (int i = 0; i < op.Operands.Length; i++)
                //if (op.Operands[i] is Location l)
                //    ResolveInclude(l);
            }
        }

        public void WriteBlocks(string outPath)
        {
            var res = DbRoot.GetPath(BinType.Assembly);
            var xRes = DbRoot.GetPath(BinType.Transform);
            //string folder = "asm", extension = "asm";
            string folderPath = Path.Combine(outPath, res.Folder);
            string transformPath = Path.Combine(outPath, xRes.Folder);
            //Directory.CreateDirectory(folderPath);

            foreach (var block in DbRoot.Blocks)
            {
                var folder = block.Group == null ? folderPath : Path.Combine(folderPath, block.Group);
                Directory.CreateDirectory(folder);

                var outFile = Path.Combine(folder, $"{block.Name}.{res.Extension}");
                if (File.Exists(outFile))
                    continue;

                using var outStream = File.Create(outFile);
                using var writer = new StreamWriter(outStream);

                if (!block.Movable)
                    writer.WriteLine("?BANK {0:X2}", block.Parts.First().Start.Bank);

                foreach (var inc in block.GetIncludes())
                    writer.WriteLine("?INCLUDE '{0}'", inc.Name); //Write includes

                writer.WriteLine(); //Empty line

                foreach (var mnem in block.Mnemonics.OrderBy(x => x.Item1.Value))
                    writer.WriteLine("!{0} {1:X4}", mnem.Item2.PadRight(30, ' '), mnem.Item1.Value);

                writer.WriteLine(); //Empty line

                IEnumerable<XformDef>? xforms = null;
                var xformFile = block.Group == null
                    ? Path.Combine(transformPath, $"{block.Name}.{xRes.Extension}")
                    : Path.Combine(transformPath, block.Group, $"{block.Name}.{xRes.Extension}");
                if (File.Exists(xformFile))
                    using (var xformStream = File.OpenRead(xformFile))
                        xforms = JsonSerializer.Deserialize<IEnumerable<XformDef>>(xformStream, DbRoot._jsonOptions);

                if (xforms != null)
                    foreach (var x in xforms.Where(x => x.Type == XformType.Lookup))
                    {
                        var table = block.Parts.First().ObjectRoot as IEnumerable<TableEntry>;
                        var tableEntry = table?.First();
                        var entries = tableEntry?.Object as IEnumerable<object>;
                        var newParts = new List<TableEntry>();
                        var newList = new List<object>();

                        newParts.Add(new() { Location = tableEntry.Location, Object = newList });
                        //RefList[tableEntry.Location] = block.Name;

                        uint eIx = 1;
                        foreach (var entry in entries.OfType<StructDef>())
                        {
                            int cIx = 0;
                            int? key = null;
                            object? value = null;
                            foreach (var obj in entry.Parts)
                            {
                                if (cIx == x.KeyIx) key = Convert.ToInt32(obj);
                                else if (cIx == x.ValueIx) value = obj;
                                cIx++;
                            }

                            if (key == null || value == null)
                                throw new("Could not locate key or value for transform");

                            var name = $"entry_{key:X2}";
                            var loc = tableEntry.Location + eIx;

                            newParts.Add(new() { Location = loc, Object = value });
                            RefList[loc] = name;

                            while (newList.Count <= key)
                                newList.Add((ushort)0);

                            newList[key.Value] = $"&{name}";

                            eIx++;
                        }

                        block.Parts.First().ObjectRoot = newParts;
                    }

                foreach (var part in block.Parts) //Iterate over each part
                {
                    _part = part;
                    _isInline = true;

                    writer.WriteLine("---------------------------------------------");
                    writer.WriteLine();

                    WriteObject(writer, part.ObjectRoot, 0);
                }

                writer.Flush();

                if (xforms?.Any(x => x.Type == XformType.Replace) == true)
                {
                    string inString;
                    outStream.Position = 0;
                    using (var reader = new StreamReader(outStream, leaveOpen: true))
                        inString = reader.ReadToEnd();

                    foreach (var x in xforms.Where(x => x.Type == XformType.Replace))
                        inString = Regex.Replace(inString, x.Key, x.Value);

                    outStream.Position = 0;
                    using (var nw = new StreamWriter(outStream, leaveOpen: true))
                        nw.Write(inString);

                    outStream.SetLength(outStream.Position);
                }
            }
        }

        private void WriteObject(StreamWriter writer, object obj, int depth, bool isBranch = false)
        {
            void Indent()
            { for (int i = 0; i < depth; i++) writer.Write("  "); }

            if (!_isInline)
            {
                writer.WriteLine();
                Indent();
            }

            if (obj is IEnumerable<TableEntry> tGroup)
            {
                //if (tGroup.Locations.Any())
                //{
                //    writer.Write($"{_part.Name} "); //Label
                //    WriteObject(writer, tGroup.Locations, depth);
                //    writer.WriteLine();
                //}

                foreach (var t in tGroup)
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
            else if (obj is IEnumerable<Op> opList)
            {
                bool first = true;
                writer.WriteLine("{");
                _isInline = true;

                foreach (var op in opList) //Process each instruction in sequence
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
                    //if (DbRoot.CopLib.Formats.TryGetValue(op.Code.Mode, out var format))
                    //{
                    //    if (op.Operands[0] is Location l)
                    //        writer.Write(format, ResolveName(l, op.Size == 2 ? 0 : op.Size - 1));
                    //    else
                    //        writer.Write(format, op.Operands);
                    //}

                    object resolveOperand(object obj, bool isBranch = false)
                    {
                        if (obj is Location l)
                            return ResolveName(l, 0, isBranch);
                        else if (obj is LocationWrapper lw)
                            return ResolveName(lw.Location, lw.Type, isBranch);
                        else if (obj is Address addr)
                        {
                            HexString hexString = (op.Size == 4 || addr.Bank == 0x7E || addr.Bank == 0x7F)
                                ? new((uint)addr) : new(addr.Offset);

                            if (DbRoot.Mnemonics.TryGetValue(hexString, out var label))
                                return label;

                            if (op.Size == 4)
                                return (uint)addr;

                            return addr.Offset;
                        }
                        return obj;
                    }

                    var ops = op.Operands;
                    if (op.CopDef != null)
                    {
                        writer.Write($"[{op.CopDef.Mnem}]");
                        var len = ops.Length;
                        if (len > 1)
                        {
                            for (int i = 1; i < len; i++)
                            {
                                writer.Write(i == 1 ? " ( " : ", ");
                                WriteObject(writer, ops[i], depth + 1, false);
                            }
                            writer.Write(" )");
                        }
                    }
                    else if (ops?.Length > 0)
                    {
                        bool isBr = op.Code.Mnem[0] == 'J' || op.Code.Mode == AddressingMode.PCRelative
                            || op.Code.Mode == AddressingMode.PCRelativeLong;

                        ops[0] = resolveOperand(ops[0], isBr);
                        if (DbRoot.CopLib.Formats.TryGetValue(op.Code.Mode, out var format))
                        {
                            if (op.Code.Mode == AddressingMode.Immediate && op.Size == 3)
                                format = format.Replace("X2", "X4");
                            writer.Write(format, ops);
                        }
                        else
                        {
                            var size = (op.Size - 1) * 2;
                            writer.Write($"${{0:X{size}}}", ops);
                        }
                    }
                    writer.WriteLine();
                    //op = op.Next;
                }

                _isInline = false;
                writer.WriteLine("}");
            }
            else if (obj is Location l) writer.Write(ResolveName(l, 0, isBranch));
            else if (obj is LocationWrapper lw) writer.Write(ResolveName(lw.Location, lw.Type, isBranch));
            else if (obj is Address addr) writer.Write("${0:X6}", (uint)addr);
            else if (obj is byte b) writer.Write("#{0:X2}", b);
            else if (obj is ushort s) writer.Write("#${0:X4}", s);
            else if (obj is byte[] a)
            {
                writer.Write("#");
                writer.Write(Convert.ToHexString(a));
            }
            //else if (obj is string[] sArr)
            //    foreach (var sa in sArr)
            //        WriteObject(writer, sa, depth);
            else if (obj is StringWrapper sw)
            {
                var str = sw.String;
                for (var ix = str.IndexOfAny(RefChar); ix >= 0; ix = str.IndexOfAny(RefChar))
                {
                    var sloc = uint.Parse(str.Substring(ix + 1, 6), NumberStyles.HexNumber);
                    var adrs = new Address((byte)(sloc >> 16), (ushort)sloc);
                    if (adrs.Space == AddressSpace.ROM)
                    {
                        var name = ResolveName(sloc, str[ix] == '^' ? AddressType.Offset : AddressType.Address, false);
                        sw.String = str = str.Replace(str.Substring(ix, 7), name);
                    }
                    else
                        throw new("Unsupported");
                    //var sLoc = Location.Parse(str.Substring(ix + 1, 6));
                    //sw.String = str = str.Replace(str.Substring(ix, 7), ResolveName(sLoc, str[ix] == '^' ? (byte)2 : (byte)3, false));
                }
                var refChar = sw.Type switch
                {
                    StringType.ASCII => '|',
                    StringType.Char => '~',
                    StringType.Wide => '`',
                    _ => throw new("Unsupported string type")
                };

                if (sw.Marker <= 0)
                    _markerTable.TryGetValue(sw.Location, out sw.Marker);

                if (sw.Marker > 0)
                {
                    int six = 0, mix = 0;
                    while (mix < sw.Marker)
                    {
                        if (str[six] == '[')
                        {
                            var eix = str.IndexOf(']', ++six);
                            var parts = str[six..eix].Split(',', ':', ' ');

                            var cmd = DbRoot.WideCommands.Values.First(x => x.Value == parts[0]);
                            foreach (var t in cmd.Types)
                            {
                                mix += t switch
                                {
                                    MemberType.Byte => 1,
                                    MemberType.Word or
                                    MemberType.Offset => 2,
                                    MemberType.Address => 3,
                                    MemberType.Binary => parts.Length - 1,
                                    _ => throw new("Unsupported")
                                };
                            }
                            six = eix + 1;
                            mix++;
                        }
                        else
                        {
                            six++;
                            mix++;
                        }
                    }
                    str = str[..six] + "[::]" + str[six..];
                }

                writer.Write(refChar);
                writer.Write(str);
                writer.Write(refChar);
            }
            else if (obj is string str)
            {
                writer.Write(str);
            }
            else if (obj is IEnumerable arr)
            {
                writer.Write('[');
                _isInline = false;
                int ix = 0;
                foreach (var o in arr)
                {
                    WriteObject(writer, o, depth + 1);
                    writer.Write("   ;{0:X2}", ix++);
                }

                writer.WriteLine();
                Indent();
                writer.Write(']');
                _isInline = true;
            }
            else writer.Write(obj);

            if (depth == 0) writer.WriteLine();

        }

        [GeneratedRegex("_([A-Fa-f0-9]{2})")]
        private static partial Regex BankRegex();
    }

    //public class TableGroup
    //{
    //    public IEnumerable<Location> Locations { get; set; }
    //    public IEnumerable<TableEntry> Blocks { get; set; }
    //}

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


    public class StringWrapper(string str, StringType type, Location loc)
    {
        public string String = str;
        //public byte[] Data;
        public StringType Type = type;
        public int Marker;
        public Location Location = loc;
    }

    public enum StringType
    {
        ASCII,
        Char,
        Wide
    }
}
