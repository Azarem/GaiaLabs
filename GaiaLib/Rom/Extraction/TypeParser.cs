using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

internal class TypeParser
{
    private readonly BlockReader _blockReader;
    private readonly RomDataReader _romDataReader;
    //private readonly ProcessorStateManager _stateManager;
    private readonly StringReader _stringReader;
    //private readonly Dictionary<int, string> _chunkTable;
    private readonly IDictionary<string, DbStringType> _stringTypes;
    private readonly ReferenceManager _referenceManager;

    public TypeParser(BlockReader blockReader)
    {
        _blockReader = blockReader;
        _referenceManager = blockReader._referenceManager;
        //_chunkTable = blockReader._structTable;
        //_stateManager = blockReader._stateManager;
        _romDataReader = blockReader._romDataReader;
        _stringReader = blockReader._stringReader;
        _stringTypes = blockReader._root.StringTypes;
    }


    public object ParseType(string typeName, Registers reg, int depth, byte? bank = null)
    {
        //var isPtr = _ptrSpace.Contains(typeName[0]);
        //string otherStr = _part.Struct ?? "Binary";
        //char? command = typeName[0];

        //Shortcut for symbolic Offsets
        if (typeName[0] == '&')
            return ParseLocation(_romDataReader.ReadUShort(), bank, typeName[1..], AddressType.Offset);

        //Shortcut for symbolic Addresses
        if (typeName[0] == '@')
            return ParseLocation(_romDataReader.ReadUShort(), _romDataReader.ReadByte(), typeName[1..], AddressType.Address);

        //Check string types
        if (_stringTypes.TryGetValue(typeName, out var stringType))
            return _stringReader.ParseString(stringType);

        //Parse raw values
        if (System.Enum.TryParse<MemberType>(typeName, true, out var mType))
            return mType switch
            {
                MemberType.Byte => _romDataReader.ReadByte(),
                MemberType.Word => ParseWordSafe(),
                MemberType.Offset => ParseLocation(_romDataReader.ReadUShort(), bank, null, AddressType.Offset),
                MemberType.Address => ParseLocation(_romDataReader.ReadUShort(), _romDataReader.ReadByte(), null, AddressType.Address),
                MemberType.Binary => ParseBinary(),
                //MemberType.String => _blockReader._stringReader.ParseASCIIString(),
                //MemberType.CharString => _blockReader._stringReader.ParseCharString(),
                //MemberType.WideString => _blockReader._stringReader.ParseWideString(),
                MemberType.Code => ParseCode(reg),
                _ => throw new("Invalid member type"),
            };

        var parentType = _blockReader._root.Structs[typeName];
        var delimiter = parentType.Delimiter;

        //On parent classes, the discriminator is the integer offset to the discriminator value.
        var discOffset = parentType.Discriminator;
        var objects = new List<object>();

        //Continue to iterate until end or delimiter is reached
        bool delReached;
        while (!(delReached = _blockReader.DelimiterReached(delimiter)))
        {
            var startPosition = _romDataReader.Position;
            var targetType = parentType;

            //If a discriminator offset is present, use it to identify the type
            if (discOffset != null)
            {
                //Get discriminator position in ROM
                var discPosition = _romDataReader.Position + discOffset.Value;

                //Get discriminator value
                var desc = _romDataReader.RomData[discPosition];

                //Advance position (hide value) if discriminator is first
                if (discOffset.Value == 0u)
                    _romDataReader.Position++;

                //Match discriminator to type. On child classes, the discriminator is the value used to identify the type.
                targetType =
                    _blockReader._root.Structs.FirstOrDefault(x =>
                            x.Value.Parent == typeName && x.Value.Discriminator == desc
                        )
                        .Value ?? parentType; //Default to parent if no match is found
            }

            var types = targetType.Types;
            if (types != null)
            {
                var memberCount = types.Length;
                var prevPosition = _romDataReader.Position;
                var parts = new object[memberCount]; //Create new member collection
                var def = new StructDef { Name = targetType.Name, Parts = parts };

                //Parse each member of the struct
                for (int i = 0; i < memberCount; i++)
                    parts[i] = ParseType(types[i], null, depth + 1);

                //Advance (hide) discriminator if it is the last member
                if (discOffset != null && discOffset == _romDataReader.Position - prevPosition)
                    _romDataReader.Position++;

                objects.Add(def);
            }

            //Roll back work if struct overflows a chunk boundary
            //SHOULD only happen for the inventory sprite map
            while (++startPosition < _romDataReader.Position)
                if (_referenceManager.ContainsStruct(startPosition))
                {
                    //_pCur -= _lCur - startLoc;
                    _romDataReader.Position = startPosition;
                    break;
                }

            //Stop if the reader should not continue
            if (!_blockReader.PartCanContinue())
                break;
        }

        //If we have reached
        if (delReached && depth == 0)
            _referenceManager.TryAddStruct(_romDataReader.Position, typeName);

        return objects;
    }

    private object ParseWordSafe()
        => _referenceManager.ContainsStruct(_romDataReader.Position + 1)
            ? (object)_romDataReader.ReadByte()
            : _romDataReader.ReadUShort();

    private byte[] ParseBinary()
    {
        //Store old position for length calculation
        var startPosition = _romDataReader.Position;

        //Advance the reader until we reach the end of the section
        do _romDataReader.Position++;
        while (_blockReader.PartCanContinue());

        //Length is determined by the new position relative to the old
        var len = _romDataReader.Position - startPosition;

        //Create buffer for the raw bytes
        var outBuffer = new byte[len];

        //Copy raw bytes from ROM to buffer
        for (int i = 0; i < len;)
            outBuffer[i++] = _romDataReader.RomData[startPosition++];

        return outBuffer;
    }

    private object ParseLocation(ushort offset, byte? bank, string? typeName, AddressType addrType)
    {
        //If bank is not provided and offset is 0, it should resolve to #$0000
        if (bank == null && offset == 0)
            return offset;

        //Bank cannot be null, instead use bank from current position.
        var resolvedBank = bank ?? (byte)(_romDataReader.Position >> 16);

        //Create the address with resolved bank
        var adrs = new Address(resolvedBank, offset);

        //If we have a system address, keep it as is
        if (adrs.Space != AddressSpace.ROM)
            return adrs;

        //Convert address to ROM location
        var loc = (int)adrs;

        //If the location is inside the current block and there is no rewrite for it...
        if (
            _blockReader._currentBlock.IsInside(loc, out _)
            && !_blockReader._root.Rewrites.ContainsKey(loc)
        )
        {
            //Normalize the type name to default to current part definition
            typeName ??= _blockReader._currentPart.Struct ?? "Binary";

            //Add the struct type to our chunk table if it is not already present
            _referenceManager.TryAddStruct(loc, typeName);

            //If the location is not already in the reference table, add it
            _referenceManager.TryAddName(loc, $"{typeName.ToLower()}_{loc:X6}");
        }

        return new LocationWrapper(loc, addrType);
    }

    private List<Op> ParseCode(Registers reg)
    {
        //Output list
        var opList = new List<Op>();

        bool first = true;
        while (_romDataReader.Position < _blockReader._partEnd)
        {
            //Check the chunk table for a new type block, but not on the first iteration
            if (first)
                first = false;
            else if (_referenceManager.ContainsStruct(_romDataReader.Position))
                break;

            //Process register adjustments before parse
            _blockReader.HydrateRegisters(reg);

            //Parse instruction
            var op = _blockReader._asmReader.ParseAsm(reg);

            //Add instruction to list
            opList.Add(op);
        }

        return opList;
    }
}

