using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Handles COP (Coprocessor) command processing
/// </summary>
public class CopCommandProcessor
{
    private readonly BlockReader _romReader;

    public CopCommandProcessor(BlockReader romReader)
    {
        _romReader = romReader;
    }

    public void ParseCopCommand(CopDef copDef, List<object> operands)
    {
        foreach (var p in copDef.Parts)
        {
            var addrType = Address.TypeFromCode(p[0]);
            var isPtr = addrType != AddressType.Unknown;
            var otherStr = isPtr ? p[1..] : _romReader._currentPart.Struct ?? "Binary";
            var memberTypeName = isPtr ? addrType.ToString() : p;

            if (!System.Enum.TryParse<MemberType>(memberTypeName, true, out var mtype))
                throw new InvalidOperationException("Cannot use structs in cop def");

            _romReader._root.Transforms.TryGetValue(_romReader._romPosition, out var xform);

            object resolve(object obj) => !string.IsNullOrEmpty(xform) ? xform : obj;

            switch (mtype)
            {
                case MemberType.Byte:
                    operands.Add(resolve(_romReader.ReadByte()));
                    break;
                    
                case MemberType.Word:
                    operands.Add(resolve(_romReader.ReadUShort()));
                    break;
                    
                case MemberType.Offset:
                    operands.Add(CreateCopLocation(_romReader.ReadUShort(), null, xform, p, isPtr, otherStr, addrType));
                    break;
                    
                case MemberType.Address:
                    operands.Add(CreateCopLocation(_romReader.ReadUShort(), _romReader.ReadByte(), xform, p, isPtr, otherStr, addrType));
                    break;
                    
                default:
                    throw new InvalidOperationException("Unsupported COP member type");
            }
        }
    }

    private object CreateCopLocation(ushort offset, byte? bank, string xform, string p, bool isPtr, string otherStr, AddressType type)
    {
        if (!string.IsNullOrEmpty(xform))
            return xform;

        if (bank == null && offset == 0)
            return offset;

        var addr = new Address(bank ?? (byte)(_romReader._romPosition >> 16), offset);
        if (addr.Space == AddressSpace.ROM)
        {
            var location = (int)addr;
            if (p != "Address" && isPtr && !_romReader._root.Rewrites.ContainsKey(location))
                _romReader.NoteType(location, otherStr, true);
                
            if (type == AddressType.Unknown)
                type = System.Enum.TryParse<AddressType>(p, true, out var parsedType) ? parsedType : AddressType.Unknown;

            return new LocationWrapper(location, type);
        }
        return addr;
    }
} 