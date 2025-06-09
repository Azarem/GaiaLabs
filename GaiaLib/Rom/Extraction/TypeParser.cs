using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

internal class TypeParser
{
    private readonly BlockReader _blockReader;
    private readonly Dictionary<int, string> _chunkTable;

    public TypeParser(BlockReader blockReader)
    {
        _blockReader = blockReader;
        _chunkTable = blockReader._chunkTable;
    }


    public object ParseType(string typeName, Registers reg, int depth, byte? bank = null)
    {
        //var isPtr = _ptrSpace.Contains(typeName[0]);
        //string otherStr = _part.Struct ?? "Binary";
        //char? command = typeName[0];

        if (typeName[0] == '&')
            return ParseLocation(_blockReader.ReadUShort(), bank, typeName[1..], '&');

        if (typeName[0] == '@')
            return ParseLocation(_blockReader.ReadUShort(), _blockReader.ReadByte(), typeName[1..], '@');

        // (otherStr, cmd, typeName) = isPtr
        //     ? (typeName[1..], typeName[0], typeName[0] == '&' ? "Offset" : "Address")
        //     : (_part.Struct ?? "Binary", (char?)null, typeName);

        //bool isPtr = str[0] == '*', isAddr = str[0] == '&';
        //var otherStr = isPtr ? str[1..] : (_part.Struct ?? "Binary");

        //char cmd = isPtr ? str[0] : (char)0;
        //if (isPtr)
        //    str = str[0] == '&' ? "Offset" : "Address";

        //Parse raw values
        if (System.Enum.TryParse<MemberType>(typeName, true, out var mType))
            return mType switch
            {
                MemberType.Byte => _blockReader.ReadByte(),
                MemberType.Word => _chunkTable.ContainsKey(_blockReader._romPosition + 1)
                    ? (object)_blockReader.ReadByte()
                    : _blockReader.ReadUShort(),
                MemberType.Offset => ParseLocation(
                    _blockReader.ReadUShort(),
                    bank,
                    _blockReader._currentPart.Struct ?? "Binary",
                    '&'
                ),
                MemberType.Address => ParseLocation(
                    _blockReader.ReadUShort(),
                    _blockReader.ReadByte(),
                    _blockReader._currentPart.Struct ?? "Binary",
                    '@'
                ),
                MemberType.Binary => ParseBinary(),
                MemberType.String => _blockReader._stringReader.ParseASCIIString(),
                MemberType.CharString => _blockReader._stringReader.ParseCharString(),
                MemberType.WideString => _blockReader._stringReader.ParseWideString(),
                MemberType.Code => ParseCode(reg),
                _ => throw new("Invalid member type"),
            };

        var parent = _blockReader._root.Structs[typeName];
        var delimiter = parent.Delimiter;

        //On parent classes, the descriminator is the integer offset to the descriminator value.
        var descOffset = parent.Descriminator;
        var objects = new List<object>();

        //Continue to iterate until end or delimiter is reached
        bool delReached;
        while (!(delReached = _blockReader.DelimiterReached(delimiter)))
        {
            var startPosition = _blockReader._romPosition;
            var targetType = parent;

            //If a descriminator offset is present, use it to identify the type
            if (descOffset != null)
            {
                //Get descriminator position in ROM
                var descPosition = _blockReader._romPosition + descOffset.Value;

                //Get descriminator value
                var desc = _blockReader.RomData[descPosition];

                //Advance position (hide) if descriminator is first
                if (descOffset.Value == 0u)
                    _blockReader._romPosition++;

                //Match descriminator to type. On child classes, the descriminator is the value used to identify the type.
                targetType =
                    _blockReader._root.Structs.FirstOrDefault(x =>
                            x.Value.Parent == typeName && x.Value.Descriminator == desc
                        )
                        .Value ?? parent; //Default to parent if no match is found
            }

            var types = targetType.Types;
            if (types != null)
            {
                var members = types.Length;
                var prevPosition = _blockReader._romPosition;
                var parts = new object[members]; //Create new member collection
                var def = new StructDef { Name = targetType.Name, Parts = parts };

                //Parse each member of the struct
                for (int i = 0; i < members; i++)
                    parts[i] = ParseType(types[i], null, depth + 1);

                //Advance (hide) descriminator if it is the last member
                if (
                    descOffset != null &&
                    descOffset == _blockReader._romPosition - prevPosition
                )
                    _blockReader._romPosition++;

                objects.Add(def);
            }

            //Roll back work if struct overflows a chunk boundary
            //SHOULD only happen for the inventory sprite map
            while (++startPosition < _blockReader._romPosition)
                if (_chunkTable.ContainsKey(startPosition))
                {
                    //_pCur -= _lCur - startLoc;
                    _blockReader._romPosition = startPosition;
                    break;
                }

            if (!_blockReader.CanContinue())
                break;
        }

        if (delReached && depth == 0)
            _chunkTable.TryAdd(_blockReader._romPosition, typeName);

        return objects;
    }


    private byte[] ParseBinary()
    {
        var startPosition = _blockReader._romPosition;

        do _blockReader._romPosition++;
        while (_blockReader.CanContinue());

        var len = _blockReader._romPosition - startPosition;
        var bytes = new byte[len];

        for (int i = 0; i < len;)
            bytes[i++] = _blockReader.RomData[startPosition++];

        return bytes;
    }

    private object ParseLocation(ushort offset, byte? bank, string typeName, char? cmd)
    {
        if (bank == null && offset == 0)
            return offset;

        //Bank cannot be null, instead use bank from current position.
        var resolvedBank = bank ?? (byte)(_blockReader._romPosition >> 16 | (cmd == '@' ? 0xC0 : 0x80));

        var adrs = new Address(resolvedBank, offset);
        if (adrs.Space != AddressSpace.ROM)
            return adrs;

        var loc = (int)adrs;

        if (
            _blockReader._currentBlock.IsInside(loc, out var part)
            && !_blockReader._root.Rewrites.ContainsKey(loc)
        )
        {
            _chunkTable.TryAdd(loc, typeName);
            _blockReader._referenceTable.TryAdd(loc, $"{typeName.ToLower()}_{loc:X6}");
        }

        return new LocationWrapper(
            loc,
            cmd != null ? Address.TypeFromCode(cmd.Value)
                : bank == null ? AddressType.Offset
                : AddressType.Address
        );
    }

    private List<Op> ParseCode(Registers reg)
    {

        var opList = new List<Op>();
        //var reg = new Registers();
        //Op prev = null, head = null;
        bool first = true;
        while (_blockReader._romPosition < _blockReader._partEnd)
        {
            var position = _blockReader._romPosition;
            if (first)
                first = false;
            else if (_chunkTable.ContainsKey(position))
                break;

            //Process branch adjustments before parse
            if (_blockReader.AccumulatorFlags.TryGetValue(position, out var acc))
                reg.AccumulatorFlag = acc;
            if (_blockReader.IndexFlags.TryGetValue(position, out var ind))
                reg.IndexFlag = ind;
            if (_blockReader.BankNotes.TryGetValue(position, out var bnk))
                reg.DataBank = bnk;
            if (_blockReader.StackPosition.TryGetValue(position, out var stack))
                reg.Stack.Location = stack;

            var op = _blockReader._asmReader.ParseAsm(reg); //Parse instruction

            opList.Add(op);
        }

        return opList;
    }
}

