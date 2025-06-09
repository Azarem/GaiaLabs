using System.Globalization;
using System.Text;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using GaiaLib.Asm;

namespace GaiaLib.Rom.Extraction;

public class StringReader(BlockReader blockReader)
{
    public static readonly char[] StringReferenceCharacters = ['~', '^'];

    private readonly BlockReader _blockReader = blockReader;

    private void ResolveCommand(DbStringCommand cmd, StringBuilder builder)
    {
        if (cmd.Types != null)
        {
            builder.Append($"[{cmd.Value}");

            bool first = true;
            foreach (var t in cmd.Types)
            {
                if (first)
                {
                    builder.Append(':');
                    first = false;
                }
                else
                    builder.Append(',');

                switch (t)
                {
                    case MemberType.Byte:
                        builder.Append($"{_blockReader.ReadByte():X}");
                        break;
                    case MemberType.Word:
                        builder.Append($"{_blockReader.ReadUShort():X}");
                        break;

                    case MemberType.Offset:
                        var loc = _blockReader.ReadUShort() | _blockReader._romPosition & 0x3F0000;
                        builder.Append($"^{loc:X6}");
                        break;

                    case MemberType.Address:
                        builder.Append($"~{_blockReader.ReadAddress():X6}");
                        break;

                    case MemberType.Binary:
                        bool sfirst = true;
                        do
                        {
                            var r = _blockReader.ReadByte();
                            if (r == cmd.Delimiter.Value)
                                break;
                            if (sfirst)
                                sfirst = false;
                            else
                                builder.Append(',');
                            builder.Append($"{r:X}");
                        } while (_blockReader.CanContinue());
                        break;

                    default:
                        throw new("Unsupported member type");
                }
            }
            builder.Append(']');
        }
        else
            builder.Append(cmd.Value);
    }

    internal StringWrapper ParseASCIIString()
    {
        var dict = _blockReader._root.StringCommands;
        var builder = new StringBuilder();
        var strLoc = _blockReader._romPosition;
        var map = _blockReader._root.Config.AsciiMap;

        do
        {
            var c = _blockReader.ReadByte();
            if (c == 0)
                break;

            if (dict.TryGetValue(c, out var cmd))
                ResolveCommand(cmd, builder);
            else
                builder.Append(map[c]);
        } while (_blockReader.CanContinue());

        //var chars = new char[builder.Length];
        //builder.CopyTo(0, chars, 0, builder.Length);
        return new(builder.ToString(), StringType.ASCII, strLoc);
    }

    internal StringWrapper ParseCharString()
    {
        var builder = new StringBuilder();
        var dict = _blockReader._root.WideCommands;
        var strLoc = _blockReader._romPosition;
        var map = _blockReader._root.Config.CharMap;

        do
        {
            var c = _blockReader.ReadByte();
            if (c == 0xCA)
                break;

            if (dict.TryGetValue(c, out var cmd))
                ResolveCommand(cmd, builder);
            else
            {
                var index = (c & 0x70) >> 1 | c & 0x07;
                builder.Append(map[index]);
            }
        } while (_blockReader.CanContinue());

        return new(builder.ToString(), StringType.Char, strLoc);
    }

    internal StringWrapper ParseWideString()
    {
        var builder = new StringBuilder();
        var dict = _blockReader._root.WideCommands;
        var strLoc = _blockReader._romPosition;
        var map = _blockReader._root.Config.WideMap;

        do
        {
            var c = _blockReader.ReadByte();
            if (c == 0xCA)
            {
                c = (byte)_blockReader.PeekByte();
                if (c == 0xCA)
                {
                    _blockReader._romPosition++;
                    builder.Append($"[{dict[0xCA].Value}]");
                }
                break;
            }

            if (dict.TryGetValue(c, out var cmd))
                ResolveCommand(cmd, builder);
            else
            {
                var index = (c & 0xE0) >> 1 | c & 0x0F;
                builder.Append(map[index]);
            }
        } while (_blockReader.CanContinue());

        return new(builder.ToString(), StringType.Wide, strLoc);
    }

    internal void ResolveString(StringWrapper sw, bool isBranch)
    {
        var str = sw.String;
        for (
            var ix = str.IndexOfAny(StringReferenceCharacters);
            ix >= 0;
            ix = str.IndexOfAny(StringReferenceCharacters, ix + 7)
        )
        {
            var sloc = int.Parse(str.Substring(ix + 1, 6), NumberStyles.HexNumber);
            var addrs = new Address((byte)(sloc >> 16), (ushort)sloc);
            if (addrs.Space == AddressSpace.ROM)
            {
                _blockReader.ResolveInclude(sloc, false);
                var name = _blockReader.ResolveName(sloc, AddressType.Unknown, false);
                var opix = name.IndexOfAny(RomProcessingConstants.Operators);
                if (opix > 0)
                {
                    var offset =
                        name[opix + 1] == 'M'
                            ? _blockReader._markerTable[sloc]
                            : int.Parse(name[(opix + 1)..], NumberStyles.HexNumber);

                    if (name[opix] == '-')
                        offset = -offset;

                    name = name[..opix];
                    var target = (uint)(sloc - offset);
                    _blockReader._currentBlock.IsOutside(sloc, out var prt);
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
}
