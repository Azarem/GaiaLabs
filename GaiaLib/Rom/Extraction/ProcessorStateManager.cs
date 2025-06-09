namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Manages CPU processor state during ROM analysis
/// </summary>
public class ProcessorStateManager
{
    private readonly Dictionary<int, bool?> _accumulatorFlags = [];
    private readonly Dictionary<int, bool?> _indexFlags = [];
    private readonly Dictionary<int, byte?> _bankNotes = [];
    private readonly Dictionary<int, byte> _stackPositions = [];

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

    public void Clear()
    {
        _accumulatorFlags.Clear();
        _indexFlags.Clear();
        _bankNotes.Clear();
        _stackPositions.Clear();
    }

    // Internal access for BlockReader compatibility
    internal Dictionary<int, bool?> AccumulatorFlags => _accumulatorFlags;
    internal Dictionary<int, bool?> IndexFlags => _indexFlags;
    internal Dictionary<int, byte?> BankNotes => _bankNotes;
    internal Dictionary<int, byte> StackPositions => _stackPositions;
} 