using GaiaLib.Asm;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Handles stack operations for various stack-related instructions
/// </summary>
internal class StackOperations
{
    private readonly Registers _registers;
    private readonly BlockReader _blockReader;

    public StackOperations(Registers registers, BlockReader romReader)
    {
        _registers = registers;
        _blockReader = romReader;
    }

    public void HandleStackOperation(string mnemonic)
    {
        switch (mnemonic)
        {
            case "PHD":
                _registers.Stack.Push(_registers.Direct ?? 0);
                break;
                
            case "PLD":
                _registers.Direct = _registers.Stack.PopUInt16();
                break;
                
            case "PHK":
                _registers.Stack.Push((byte)(_blockReader._romDataReader.Position >> 16 | 0x80));
                break;
                
            case "PHB":
                _registers.Stack.Push(_registers.DataBank ?? 0x81);
                break;
                
            case "PLB":
                _registers.DataBank = _registers.Stack.PopByte();
                break;
                
            case "PHP":
                _registers.Stack.Push((byte)_registers.StatusFlags);
                break;
                
            case "PLP":
                _registers.StatusFlags = (StatusFlags)_registers.Stack.PopByte();
                break;

            case "PHA":
                HandleAccumulatorPush();
                break;

            case "PLA":
                HandleAccumulatorPull();
                break;

            case "PHX":
                HandleXIndexPush();
                break;

            case "PLX":
                HandleXIndexPull();
                break;

            case "PHY":
                HandleYIndexPush();
                break;

            case "PLY":
                HandleYIndexPull();
                break;

            case "XBA":
                HandleExchangeBytes();
                break;
        }
    }

    private void HandleAccumulatorPush()
    {
        if (_registers.AccumulatorFlag == true)
            _registers.Stack.Push((byte)(_registers.Accumulator ?? 0));
        else
            _registers.Stack.Push(_registers.Accumulator ?? 0);
    }

    private void HandleAccumulatorPull()
    {
        if (_registers.AccumulatorFlag == true)
            _registers.Accumulator = (ushort)((_registers.Accumulator ?? 0) & 0xFF00u | _registers.Stack.PopByte());
        else
            _registers.Accumulator = _registers.Stack.PopUInt16();
    }

    private void HandleXIndexPush()
    {
        if (_registers.IndexFlag == true)
            _registers.Stack.Push((byte)(_registers.XIndex ?? 0));
        else
            _registers.Stack.Push(_registers.XIndex ?? 0);
    }

    private void HandleXIndexPull()
    {
        if (_registers.IndexFlag == true)
            _registers.XIndex = (ushort)((_registers.XIndex ?? 0) & 0xFF00u | _registers.Stack.PopByte());
        else
            _registers.XIndex = _registers.Stack.PopUInt16();
    }

    private void HandleYIndexPush()
    {
        if (_registers.IndexFlag == true)
            _registers.Stack.Push((byte)(_registers.YIndex ?? 0));
        else
            _registers.Stack.Push(_registers.YIndex ?? 0);
    }

    private void HandleYIndexPull()
    {
        if (_registers.IndexFlag == true)
            _registers.YIndex = (ushort)((_registers.YIndex ?? 0) & 0xFF00u | _registers.Stack.PopByte());
        else
            _registers.YIndex = _registers.Stack.PopUInt16();
    }

    private void HandleExchangeBytes()
    {
        _registers.Accumulator = (ushort)((_registers.Accumulator ?? 0) >> 8 | (_registers.Accumulator ?? 0) << 8);
    }
} 