using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Handles COP (Coprocessor) command processing
/// </summary>
internal class CopCommandProcessor
{
    private readonly BlockReader _blockReader;
    private readonly RomDataReader _romDataReader;

    public CopCommandProcessor(BlockReader blockReader)
    {
        _blockReader = blockReader;
        _romDataReader = blockReader._romDataReader;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="copDef"></param>
    /// <param name="operands"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ParseCopCommand(CopDef copDef, List<object> operands)
    {
        foreach (var partStr in copDef.Parts)
        {
            //Use the first character to determine the address type (for pointers)
            var addrType = Address.TypeFromCode(partStr[0]);
            var isPtr = addrType != AddressType.Unknown;
            //Reference type is the target of a pointer from partStr, or the struct type if not a pointer
            var referenceType = isPtr ? partStr[1..] : _blockReader._currentPart.Struct ?? "Binary";
            //Member type resolves to the underlying pointer type, or partStr
            var memberTypeName = isPtr ? addrType.ToString() : partStr;

            //Resolve member type name to a MemberType enum. (for Offset / Address overlap)
            if (!System.Enum.TryParse<MemberType>(memberTypeName, true, out var memberType))
                throw new InvalidOperationException("Cannot use structs in cop def"); //Only basic types are allowed in COP definitions

            //If there is a label, ignore reading and use the label instead
            if (_blockReader._root.Labels.TryGetValue(_romDataReader.Position, out var label))
            {
                _romDataReader.Position += memberType switch
                {
                    MemberType.Byte => 1,
                    MemberType.Word => 2,
                    MemberType.Offset => 2,
                    MemberType.Address => 3,
                    _ => throw new InvalidOperationException("Unsupported COP member type")
                };
                operands.Add(label);
            }
            else
                operands.Add(memberType switch
                {
                    MemberType.Byte => _romDataReader.ReadByte(),
                    MemberType.Word => _romDataReader.ReadUShort(),
                    MemberType.Offset => CreateCopLocation(_romDataReader.ReadUShort(), null, partStr, isPtr, referenceType, addrType),
                    MemberType.Address => CreateCopLocation(_romDataReader.ReadUShort(), _romDataReader.ReadByte(), partStr, isPtr, referenceType, addrType),
                    _ => throw new InvalidOperationException("Unsupported COP member type")
                });

        }
    }

    private object CreateCopLocation(ushort offset, byte? bank, string partStr, bool isPtr, string otherStr, AddressType type)
    {
        if (bank == null && offset == 0)
            return offset;

        var addr = new Address(bank ?? (byte)(_romDataReader.Position >> 16), offset);
        if (addr.Space == AddressSpace.ROM)
        {
            var location = (int)addr;
            if (partStr != "Address" && isPtr && !_blockReader._root.Rewrites.ContainsKey(location))
                _blockReader.NoteType(location, otherStr, true);

            //When address is unknown, try to use the part string (for Offset or Address)
            if (type == AddressType.Unknown)
                type = System.Enum.TryParse<AddressType>(partStr, true, out var parsedType) ? parsedType : AddressType.Unknown;

            return new LocationWrapper(location, type);
        }
        return addr;
    }
} 