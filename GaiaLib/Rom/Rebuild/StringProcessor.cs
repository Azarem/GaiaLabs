
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System.Globalization;

namespace GaiaLib.Rom.Rebuild;

internal class StringProcessor : IDisposable
{
    private readonly MemoryStream memStream;
    private readonly Assembler _assembler;
    private readonly DbRoot _root;

    public StringProcessor(Assembler assembler)
    {
        _assembler = assembler;
        _root = assembler._root;
        memStream = new MemoryStream();
    }

    public void Dispose()
    {
        memStream.Dispose();
        GC.SuppressFinalize(this);
    }

    public void ConsumeString()
    {
        int ix;
        string? str = null;

        //Get character code of string type
        var typeChar = _assembler._lineBuffer[0];

        //Find last index of character code
        var endIx = _assembler._lineBuffer.IndexOf(typeChar, 1);
        if (endIx >= 0) //If end found
        {
            //Take the line up until the type code
            str = _assembler._lineBuffer[1..endIx];
            //Line takes content after and code
            _assembler._lineBuffer = _assembler._lineBuffer[(endIx + 1)..].TrimStart(RomProcessingConstants.CommaSpace);
        }
        else
        {
            //Take the remaining line
            str = _assembler._lineBuffer[1..];
            _assembler._lineBuffer = "";
        }

        //Reset memory stream for new string
        memStream.Position = 0;
        memStream.SetLength(0);

        var stringType = _root.StringCharLookup[typeChar];
        ProcessString(str, stringType);
    }


    private void flushBuffer(DbStringType stringType, bool wrap = false)
    {
        var buffer = memStream.GetBuffer();
        var size = (int)memStream.Length;
        if (size > 0)
        {
            var newBuffer = new byte[size];
            Array.Copy(buffer, newBuffer, size);
            _assembler.currentBlock.ObjList.Add(newBuffer); ///TODO: Check this, used to be StringWrapper
            _assembler.currentBlock.Size += size;
            memStream.Position = 0;
            memStream.SetLength(0);
        }
    }


    void ProcessString(
        string str,
        DbStringType stringType
    )
    {
        var dict = stringType.Commands;
        var charMap = stringType.CharacterMap;
        //string[]? accentMap,
        var shift = stringType.ShiftUp;
        DbStringCommand? lastCmd = null;

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
                    flushBuffer(stringType, true);
                    _assembler.currentBlock.ObjList.Add(
                        new StringMarker { Offset = _assembler.currentBlock.Size }
                    );
                    continue;
                }

                var cmd = dict.Values.FirstOrDefault(x => x.Value == parts[0]);
                if (cmd != null)
                {
                    lastCmd = cmd;
                    memStream.WriteByte((byte)cmd.Key);
                    ProcessStringCommand(cmd, stringType, parts);
                    continue;
                }
            }

            lastCmd = null;
            //processChar(c);

            //Process extra string layers
            if (ApplyLayers(c, stringType))
                continue;
        }

        //Terminate string
        if (lastCmd == null || !lastCmd.Halt)
            memStream.WriteByte((byte)stringType.Terminator);

        flushBuffer(stringType, true);
    }

    private bool ApplyLayers(char c, DbStringType stringType)
    {
        if (ApplyMap(c, stringType.CharacterMap, stringType.ShiftUp))
            return true;

        if (stringType.Layers != null)
            foreach (var layer in stringType.Layers)
                if (ApplyMap(c, layer.Map, x => (byte)(x + layer.Base)))
                    return true;
        return false;
    }

    private bool ApplyMap(char c, string[] map, Func<byte, byte> shift)
    {
        for (int i = 0, len = map.Length; i < len; i++)
        {
            var v = map[i];
            if (v != null && c == v[0])
            {
                memStream.WriteByte(shift((byte)i));
                return true;
            }
        }
        return false;
    }

    private void ProcessStringCommand(DbStringCommand cmd, DbStringType stringType, string[] parts)
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
                    _assembler.currentBlock.ObjList.Add(parts[pix]);
                    _assembler.currentBlock.Size += cmd.Types[y] == MemberType.Offset ? 2 : 3;
                    break;
            }
        }

        if (hasPointer)
            flushBuffer(stringType, false);
    }

}
