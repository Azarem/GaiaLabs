using GaiaLib.Asm;
using static GaiaLib.Types.RomProcessingConstants;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Manages CPU processor state during ROM analysis
/// </summary>
internal class ProcessorStateManager
{
    internal readonly Dictionary<int, bool?> _accumulatorFlags = [];
    internal readonly Dictionary<int, bool?> _indexFlags = [];
    internal readonly Dictionary<int, byte?> _bankNotes = [];
    internal readonly Dictionary<int, byte> _stackPositions = [];

    public void HydrateRegisters(int position, Registers reg)
    {
        if (_accumulatorFlags.TryGetValue(position, out var acc))
            reg.AccumulatorFlag = acc;

        if (_indexFlags.TryGetValue(position, out var ind))
            reg.IndexFlag = ind;

        if (_bankNotes.TryGetValue(position, out var bnk))
            reg.DataBank = bnk;

        if (_stackPositions.TryGetValue(position, out var stack))
            reg.Stack.Location = stack;
    }

    public bool? GetAccumulatorFlag(int location)
    {
        return _accumulatorFlags.TryGetValue(location, out var value) ? value : null;
    }

    public void SetAccumulatorFlag(int location, bool? value)
    {
        _accumulatorFlags[location] = value;
    }

    public bool TryAddAccumulatorFlag(int location, bool? value)
    {
        return _accumulatorFlags.TryAdd(location, value);
    }

    public bool? GetIndexFlag(int location)
    {
        return _indexFlags.TryGetValue(location, out var value) ? value : null;
    }

    public void SetIndexFlag(int location, bool? value)
    {
        _indexFlags[location] = value;
    }

    public bool TryAddIndexFlag(int location, bool? value)
    {
        return _indexFlags.TryAdd(location, value);
    }

    public byte? GetBankNote(int location)
    {
        return _bankNotes.TryGetValue(location, out var value) ? value : null;
    }

    public void SetBankNote(int location, byte? value)
    {
        _bankNotes[location] = value;
    }

    public byte? GetStackPosition(int location)
    {
        return _stackPositions.TryGetValue(location, out var value) ? value : null;
    }

    public void SetStackPosition(int location, byte value)
    {
        _stackPositions[location] = value;
    }

    public bool TryAddStackPosition(int location, byte value)
    {
        return _stackPositions.TryAdd(location, value);
    }

}