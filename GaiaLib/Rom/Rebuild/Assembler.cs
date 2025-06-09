
using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System.Globalization;
using System.IO;
using System.Net;

namespace GaiaLib.Rom.Rebuild;

public class Assembler : IDisposable
{
    public readonly DbRoot _root;
    private readonly FileStream inStream;
    private readonly StreamReader reader;
    public readonly MemoryStream memStream;

    public string? line = "";
    public HashSet<string> includes = new();
    SortedDictionary<string, string?> tags = new(StringSizeComparer.Instance);
    public List<AsmBlock> blocks = [];
    internal AsmBlock currentBlock;
    internal int lineCount;
    internal int blockIndex;
    internal int? lastDelimiter;
    internal byte? reqBank;

    public Assembler(DbRoot dbRoot, string filePath)
    {
        _root = dbRoot;
        inStream = File.OpenRead(filePath);
        reader = new StreamReader(inStream);
        memStream = new MemoryStream();
    }

    public void Dispose()
    {
        reader.Dispose();
        inStream.Dispose();
        memStream.Dispose();
        GC.SuppressFinalize(this);
    }

    public (List<AsmBlock> blocks, HashSet<string> includes, byte? reqBank) ParseAssembly(int startLoc)
    {
        using var reader = new StreamReader(inStream);

        includes = new HashSet<string>();
        blocks = new List<AsmBlock>();
        currentBlock = null;
        //AsmBlock? target;
        tags = new SortedDictionary<string, string?>(StringSizeComparer.Instance);
        //memStream.Position = 0;

        blockIndex = 0;
        lineCount = 0;

        reqBank = null;
        //string? lastStruct = null;
        lastDelimiter = null;

        blocks.Add(currentBlock = new AsmBlock { Location = startLoc });

        var state = new AssemblerState(this);
        state.ProcessText();

        return (blocks, includes, reqBank);
    }

    public string? GetLine()
    {
        //This can happen
        if (line == null)
            return null;

        //Keep processing what we already have
        if (line.Length > 0)
            return line;

        Read:
        line = reader.ReadLine();
        lineCount++;

        //This can happen
        if (line == null)
            return null;

        lineCount++;

        //Ignore comments
        TrimComments("--");
        TrimComments(";");
        TrimComments("//");

        //Trim
        line = line.Trim(RomProcessingConstants.CommaSpace);

        //This can happen
        if (line.Length == 0)
            goto Read;

        //Process directives
        if (line[0] == '?')
        {
            ProcessDirectives();
            goto Read;
        }

        //Process tags
        if (line[0] == '!')
        {
            ProcessTags();
            goto Read;
        }

        ResolveTags();

        return line;
    }

    private void TrimComments(string sequence)
    {
        var index = line.IndexOf(sequence);
        if (index >= 0)
        {
            var cix = line.IndexOfAny(RomProcessingConstants.StringSpace);
            if (cix < 0 || cix > index || (cix = line.LastIndexOf(line[cix])) < index)
                line = line[..index];
        }
    }


    private void ProcessDirectives()
    {
        //Find end of directive
        var endIx = line.IndexOfAny(RomProcessingConstants.CommaSpace);

        //Default to line length if no end characters
        if (endIx < 0)
            endIx = line.Length;

        var value = line[endIx..].TrimStart(RomProcessingConstants.CommaSpace);

        switch (line[1..endIx].ToUpper())
        {
            //This is taken care of by the loader
            case "BANK":
                reqBank = byte.Parse(value, NumberStyles.HexNumber);
                break;

            case "INCLUDE":
                if (value.Length > 0)
                    includes.Add(value.ToUpper().Replace("'", ""));
                break;
        }
    }

    private void ProcessTags()
    {
        //Remove the '!' prefix and trim leading spaces
        line = line[1..].TrimStart(RomProcessingConstants.CommaSpace);

        //Process as many pairs as we can
        while (line.Length > 0)
        {
            string name = line;
            string? value = null;
            var endIx = line.IndexOfAny(RomProcessingConstants.CommaSpace);

            if (endIx >= 0)
            {
                name = line[..endIx];
                value = line[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
                if ((endIx = value.IndexOfAny(RomProcessingConstants.CommaSpace)) >= 0)
                {
                    line = value[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
                    value = value[..endIx];
                }
                else
                    line = "";
            }
            else
                line = "";

            tags[name] = value;
        }
    }

    private void ResolveTags()
    {
        int ix;
        foreach (var tag in tags)
            while ((ix = line.IndexOf(tag.Key, StringComparison.CurrentCultureIgnoreCase)) >= 0)
                line = line[..ix] + tag.Value + line[(ix + tag.Key.Length)..];
    }




    public object ParseOperand(string opnd) =>
        opnd.Length switch
        {
            2 when byte.TryParse(opnd, NumberStyles.HexNumber, null, out var b) => b,
            4 when ushort.TryParse(opnd, NumberStyles.HexNumber, null, out var s) => s,
            6 when uint.TryParse(opnd, NumberStyles.HexNumber, null, out var i) => i,
            _ => opnd,
        };

    public void ProcessRawData()
    {
        bool reverse = false;

        //Immediate binary marker
        if (line[0] == '#')
            line = line[1..];

        //Reverse binary marker
        if (line[0] == '$')
        {
            reverse = true;
            line = line[1..];
        }

        string hex;
        var symbolIx = line.IndexOfAny(RomProcessingConstants.SymbolSpace);
        if (symbolIx >= 0)
        {
            hex = line[..symbolIx];
            line = line[symbolIx..].TrimStart(RomProcessingConstants.CommaSpace);
        }
        else
        {
            hex = line;
            line = "";
        }

        if (hex.Length > 0)
        {
            //Keep string values of address markers so they can be resolved later
            if (RomProcessingConstants.AddressSpace.Contains(hex[0]))
            {
                currentBlock.ObjList.Add(hex);
                currentBlock.Size += RomProcessingConstants.GetSize(hex);
            }
            else
            {
                var data = Convert.FromHexString(hex);
                if (data.Length == 1)
                {
                    currentBlock.ObjList.Add(data[0]);
                    currentBlock.Size++;
                }
                else
                {
                    if (reverse)
                        for (int x = 0, y = data.Length; --y > x;)
                        {
                            var sample = data[x];
                            data[x++] = data[y];
                            data[y] = sample;
                        }

                    if (data.Length == 2)
                    {
                        currentBlock.ObjList.Add((ushort)(data[0] | data[1] << 8));
                        currentBlock.Size += 2;
                    }
                    else
                    {
                        currentBlock.ObjList.Add(data);
                        currentBlock.Size += data.Length;
                    }
                }
            }
        }
    }

    public void ConsumeString()
    {
        int ix;
        string? str = null;

        //Get character code of string type
        var typeChar = line[0];

        //Find last index of character code
        var endIx = line.IndexOf(typeChar, 1);
        if (endIx >= 0) //If end found
        {
            //Take the line up until the type code
            str = line[1..endIx];
            //Line takes content after and code
            line = line[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
        }
        else
        {
            //Take the remaining line
            str = line[1..];
            line = "";
        }

        //Reset memory stream for new string
        memStream.Position = 0;
        memStream.SetLength(0);

        byte? lastCmd = null;
        switch (typeChar)
        {
            case '`':
                ProcessString(
                    str,
                     StringType.Wide,
                    _root.WideCommands,
                    _root.Config.WideMap,
                    i => (byte)((i & 0x70) << 1 | i & 0x0F)
                );
                break;

            case '~':
                ProcessString(
                    str,
                    StringType.Char,
                    _root.WideCommands,
                    _root.Config.CharMap,
                    i => (byte)((i & 0x38) << 1 | i & 0x07)
                );
                break;

            default:
                ProcessString(str, StringType.ASCII, _root.StringCommands, _root.Config.AsciiMap, i => i);
                //memStream.WriteByte(0);
                break;

                //default:
                //    throw new("Unsupported string type");
        }
    }


    private void flushBuffer(StringType stringType, bool wrap = false)
    {
        var buffer = memStream.GetBuffer();
        var size = (int)memStream.Length;
        if (size > 0)
        {
            var newBuffer = new byte[size];
            Array.Copy(buffer, newBuffer, size);
            currentBlock.ObjList.Add(
                wrap && stringType == StringType.Wide
                    ? new StringEntry
                    {
                        Data = newBuffer,
                        Block = currentBlock,
                        Index = currentBlock.ObjList.Count,
                        Size = newBuffer.Length,
                    }
                    : newBuffer
            );
            currentBlock.Size += size;
            memStream.Position = 0;
            memStream.SetLength(0);
        }
    }


    void ProcessString(
        string str,
        StringType stringType,
        IDictionary<int, DbStringCommand> dict,
        string[]? charMap,
        Func<byte, byte>? shift
    )
    {
        byte? lastCmd = null;

        for (int x = 0; x < str.Length; x++)
        {
            var c = str[x];
            if (c == '[')
            {
                var endIx = str.IndexOf(']', x + 1);
                var splitChars = new char[] { ':', ',', ' ' };
                var parts = str[(x + 1)..endIx]
                    .Split(
                        splitChars,
                        StringSplitOptions.RemoveEmptyEntries
                    );

                x = endIx;

                //Marker
                if (parts.Length == 0)
                {
                    flushBuffer(stringType, stringType == StringType.Wide);
                    currentBlock.ObjList.Add(
                        new StringMarker { Offset = currentBlock.Size }
                    );
                    continue;
                }

                var cmd = dict.Values.FirstOrDefault(x => x.Value == parts[0]);
                if (cmd != null)
                {
                    lastCmd = (byte)cmd.Key;
                    memStream.WriteByte(lastCmd.Value);
                    ProcessStringCommand(cmd, stringType, parts);
                    continue;
                }
            }

            lastCmd = null;
            //processChar(c);

            for (int i = 0, len = charMap.Length; i < len; i++)
            {
                var v = charMap[i];
                if (v != null && c == v[0])
                {
                    memStream.WriteByte(shift((byte)i));
                    break;
                }
            }
        }

        //Terminate string
        if (stringType == StringType.ASCII)
            memStream.WriteByte(0);
        else if (lastCmd == null || !RomProcessingConstants.EndChars.Contains(lastCmd.Value))
            memStream.WriteByte(0xCA);

        flushBuffer(stringType, true);
    }

    private void ProcessStringCommand(DbStringCommand cmd, StringType stringType, string[] parts)
    {
        var hasPointer = cmd.Types.Contains(MemberType.Address) || cmd.Types.Contains(MemberType.Offset);
        if (hasPointer)
            flushBuffer(stringType, true);

        for (int y = 0, pix = 1; y < cmd.Types.Length; y++, pix++)
        {
            switch (cmd.Types[y])
            {
                case MemberType.Byte:
                    memStream.WriteByte(byte.Parse(parts[pix], NumberStyles.HexNumber));
                    break;

                case MemberType.Word:
                    var us = ushort.Parse(parts[pix], NumberStyles.HexNumber);
                    memStream.WriteByte((byte)us);
                    memStream.WriteByte((byte)(us >> 8));
                    break;

                case MemberType.Binary:
                    while (pix < parts.Length)
                    {
                        var ch = byte.Parse(parts[pix], NumberStyles.HexNumber);
                        memStream.WriteByte(ch);
                        pix++;
                    }
                    memStream.WriteByte((byte)cmd.Delimiter.Value);
                    break;

                case MemberType.Offset:
                case MemberType.Address:
                    //Have to keep these for later since we don't have lookups yet
                    flushBuffer(stringType, false);
                    currentBlock.ObjList.Add(parts[pix]);
                    currentBlock.Size += cmd.Types[y] == MemberType.Offset ? 2 : 3;
                    break;
            }
        }

        if (hasPointer)
            flushBuffer(stringType, false);
    }


}
