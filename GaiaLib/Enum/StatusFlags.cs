namespace GaiaLib.Enum;

// [ n v m x d i z c ]
// n = Negative
// v = Overflow
// m = Accumulator Mode
// x = Index Mode
// d = Decimal Mode
// i = IRQ Disable
// z = Zero
// c = Carry

[Flags]
public enum StatusFlags : byte
{
    /// <summary>
    /// Clear before starting addition or subtraction.
    /// Arithmetic overflow:
    /// [addition - carry out of high bit:]
    /// 0 = no carry
    /// 1 = carry
    /// [subtraction - borrow required to subtract:]
    /// 0 = borrow required
    /// 1 = no borrow required
    /// [Logic:]
    /// receives bit shifted or rotated out;
    /// source of bit rotated in
    /// </summary>
    Carry = 0x01,
    /// <summary>
    /// Indicates zero or non-zero result:
    /// 0 = non-zero result
    /// 1 = zero result
    /// </summary>
    Zero = 0x02,
    /// <summary>
    /// Enables or disables processor's IRQ interrupt line:
    /// Set to disable interrupts by masking the IRQ line
    /// Clear to enable IRQ interrupts
    /// </summary>
    IrqDisable = 0x04,
    /// <summary>
    /// Determines mode for add/subtract (not increment/decrement, though):
    /// Set to force decimal operation (BCD)
    /// Clear to return to binary operation
    /// </summary>
    DecimalMode = 0x08,
    IndexMode = 0x10,
    AccumulatorMode = 0x20,
    /// <summary>
    /// Clear to reverse "set-overflow" hardware input.
    /// Indicates invalid carry into high bit of arithmetic
    /// result (two's-complement overflow):
    /// 0 = two's-complement result ok
    /// 1 = error if two's-complement arithmetic
    /// </summary>
    Overflow = 0x40,
    /// <summary>
    /// Reflects most significant bit of result
    /// (the sign of a two's-complement binary number):
    /// 0 = high bit clear (positive result)
    /// 1 = high bit set (negative result)
    /// </summary>
    Negative = 0x80
}
