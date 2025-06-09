using System.Text.Json.Serialization;

namespace GaiaLib.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AddressingMode
{
    Accumulator,
    Immediate,
    Absolute,
    AbsoluteIndirect,
    AbsoluteIndirectLong,
    DirectPage,
    AbsoluteIndexedX,
    AbsoluteIndexedY,
    AbsoluteIndexedIndirect,
    DirectPageIndexedX,
    DirectPageIndexedY,
    DirectPageIndexedIndirectX,
    Implied,
    StackRelative,
    StackRelativeIndirectIndexedY,
    DirectPageIndirect,
    AbsoluteLong,
    AbsoluteLongIndexedX,
    DirectPageIndirectLong,
    DirectPageIndirectLongIndexedY,
    DirectPageIndirectIndexedY,
    BlockMove,
    PCRelative,
    PCRelativeLong,
    StackInterrupt,
    Stack
}
