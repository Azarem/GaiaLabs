using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System.Collections;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GaiaLib.Rom.Extraction;

internal class BlockWriter
{
    public DbRoot _root;
    public BlockReader _blockReader;
    internal readonly ReferenceManager _referenceManager;

    private bool _isInline;
    private DbPart _currentPart;

    public BlockWriter(BlockReader reader)
    {
        _blockReader = reader;
        _root = reader._root;
        _referenceManager = reader._referenceManager;
    }

    public void WriteBlocks(string outPath)
    {
        var res = _root.GetPath(BinType.Assembly);
        var xRes = _root.GetPath(BinType.Transform);

        string folderPath = Path.Combine(outPath, res.Folder);
        string transformPath = Path.Combine(outPath, xRes.Folder);

        foreach (var block in _root.Blocks)
        {
            var groupedFolderPath =
                block.Group == null ? folderPath : Path.Combine(folderPath, block.Group);
            Directory.CreateDirectory(groupedFolderPath);

            var outFile = Path.Combine(groupedFolderPath, $"{block.Name}.{res.Extension}");
            if (File.Exists(outFile))
                continue;

            using var outStream = File.Create(outFile);
            using var writer = new StreamWriter(outStream);

            if (!block.Movable)
                writer.WriteLine("?BANK {0:X2}", block.Parts.First().Start >> 16);

            foreach (var inc in block.GetIncludes())
                writer.WriteLine("?INCLUDE '{0}'", inc.Name); //Write includes

            writer.WriteLine(); //Empty line

            foreach (var mnem in block.Mnemonics.OrderBy(x => x.Key))
                writer.WriteLine("!{0} {1:X4}", mnem.Value.PadRight(30, ' '), mnem.Key);

            writer.WriteLine(); //Empty line

            IEnumerable<XformDef>? xforms = null;
            var xformFile =
                block.Group == null
                    ? Path.Combine(transformPath, $"{block.Name}.{xRes.Extension}")
                    : Path.Combine(
                        transformPath,
                        block.Group,
                        $"{block.Name}.{xRes.Extension}"
                    );
            if (File.Exists(xformFile))
                using (var xformStream = File.OpenRead(xformFile))
                    xforms = JsonSerializer.Deserialize<IEnumerable<XformDef>>(
                        xformStream,
                        DbRoot.JsonOptions
                    );

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

                    int eIx = 1;
                    foreach (var entry in entries.OfType<StructDef>())
                    {
                        int cIx = 0;
                        int? key = null;
                        object? value = null;
                        foreach (var obj in entry.Parts)
                        {
                            if (cIx == x.KeyIx)
                                key = Convert.ToInt32(obj);
                            else if (cIx == x.ValueIx)
                                value = obj;
                            cIx++;
                        }

                        if (key == null || value == null)
                            throw new("Could not locate key or value for transform");

                        var name = $"entry_{key:X2}";
                        var loc = tableEntry.Location + eIx;

                        newParts.Add(new() { Location = loc, Object = value });

                        //Force labels to match the new name
                        _referenceManager._nameTable[loc] = name;

                        while (newList.Count <= key)
                            newList.Add((ushort)0);

                        newList[key.Value] = $"&{name}";

                        eIx++;
                    }

                    block.Parts.First().ObjectRoot = newParts;
                }

            foreach (var part in block.Parts) //Iterate over each part
            {
                _currentPart = part;
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

    private object ResolveOperand(Op op, object obj, bool isBranch = false)
    {
        if (obj is int location)
            return _blockReader.ResolveName(location, 0, isBranch);
        if (obj is LocationWrapper lw)
            return _blockReader.ResolveName(lw.Location, lw.Type, isBranch);
        else if (obj is Address addr)
        {
            if (op.Size == 4)
                return addr;

            if (addr.IsCodeBank && addr.Offset < Address.UpperBank)
                if (_root.Mnemonics.TryGetValue(addr.Offset, out var label))
                    return label;

            return addr.Offset;
        }
        return obj;
    }

    private void WriteObject(StreamWriter writer, object obj, int depth, bool isBranch = false)
    {
        void Indent()
        {
            for (int i = 0; i < depth; i++)
                writer.Write("  ");
        }

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
                writer.Write(
                    $"{(_referenceManager.TryGetName(t.Location, out var s) ? s : $"loc_{t.Location:X6}")} "
                );
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
                if (first)
                    first = false;
                else
                    writer.Write(", ");
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
                else if (_referenceManager.TryGetName(op.Location, out var label)) //Check for code label
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

                //object resolveOperand(object obj, bool isBranch = false)
                //{
                //    // if (obj is Location l)
                //    //     return ResolveName(l, 0, isBranch);
                //    if (obj is LocationWrapper lw)
                //        return ResolveName(lw.Location, lw.Type, isBranch);
                //    else if (obj is Address addr)
                //    {
                //        if (op.Size == 4)
                //            return (int)addr;

                //        if (addr.IsCodeBank && addr.Offset < UpperBank)
                //            if (DbRoot.Mnemonics.TryGetValue(addr.Offset, out var label))
                //                return label;

                //        return addr.Offset;
                //    }
                //    return obj;
                //}

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
                    bool isBr =
                        op.Code.Mnem[0] == 'J'
                        || op.Code.Mode == AddressingMode.PCRelative
                        || op.Code.Mode == AddressingMode.PCRelativeLong;

                    ops[0] = ResolveOperand(op, ops[0], isBr);
                    if (_root.Config.AsmFormats.TryGetValue(op.Code.Mode, out var format))
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
        //else if (obj is Location l)
        //    writer.Write(ResolveName(l, 0, isBranch));
        else if (obj is LocationWrapper lw)
            writer.Write(_blockReader.ResolveName(lw.Location, lw.Type, isBranch));
        else if (obj is Address addr)
            writer.Write("${0}", addr.ToString());
        else if (obj is byte b)
            writer.Write("#{0:X2}", b);
        else if (obj is ushort s)
            writer.Write("#${0:X4}", s);
        else if (obj is int i)
            writer.Write("#${0:X4}", i);
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
            for (
                var ix = str.IndexOfAny(StringReader.StringReferenceCharacters);
                ix >= 0;
                ix = str.IndexOfAny(StringReader.StringReferenceCharacters, ix + 7)
            )
            {
                var sloc = int.Parse(str.Substring(ix + 1, 6), NumberStyles.HexNumber);
                var adrs = new Address((byte)(sloc >> 16), (ushort)sloc);
                if (adrs.Space == AddressSpace.ROM)
                {
                    var name = _blockReader.ResolveName(
                        (int)adrs,
                        str[ix] == '^' ? AddressType.Offset : AddressType.Address,
                        false
                    );
                    sw.String = str = str.Replace(str.Substring(ix, 7), name);
                }
                else
                    throw new("Unsupported");
                //var sLoc = Location.Parse(str.Substring(ix + 1, 6));
                //sw.String = str = str.Replace(str.Substring(ix, 7), ResolveName(sLoc, str[ix] == '^' ? (byte)2 : (byte)3, false));
            }
            var refChar = sw.Type.Delimiter;

            if (sw.Marker <= 0)
                _referenceManager.TryGetMarker(sw.Location, out sw.Marker);

            if (sw.Marker > 0)
            {
                int six = 0,
                    mix = 0;
                while (mix < sw.Marker)
                {
                    if (str[six] == '[')
                    {
                        var eix = str.IndexOf(']', ++six);
                        var parts = str[six..eix].Split(',', ':', ' ');

                        var cmd = sw.Type.Commands.Values.First(x => x.Value == parts[0]);
                        foreach (var t in cmd.Types)
                        {
                            mix += t switch
                            {
                                MemberType.Byte => 1,
                                MemberType.Word or MemberType.Offset => 2,
                                MemberType.Address => 3,
                                MemberType.Binary => parts.Length - 1,
                                _ => throw new("Unsupported"),
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
        else
            writer.Write(obj);

        if (depth == 0)
            writer.WriteLine();
    }

}
