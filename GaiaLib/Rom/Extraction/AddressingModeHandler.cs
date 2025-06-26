using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Handles different addressing modes for assembly instructions
/// </summary>
internal class AddressingModeHandler
{
    private readonly BlockReader _blockReader;
    private readonly TransformProcessor _transformProcessor;
    private readonly CopCommandProcessor _copProcessor;
    private readonly RomDataReader _dataReader;

    public AddressingModeHandler(BlockReader blockReader, TransformProcessor transformProcessor)
    {
        _blockReader = blockReader;
        _transformProcessor = transformProcessor;
        _copProcessor = new CopCommandProcessor(blockReader);
        _dataReader = blockReader._romDataReader;
    }

    public List<object> ProcessAddressingMode(OpCode code, AsmReader.OperationContext context, Registers reg)
    {
        var operands = new List<object>(8);

        switch (code.Mode)
        {
            case AddressingMode.Stack:
            case AddressingMode.Implied:
                HandleStackOrImpliedMode(code.Mnem, reg);
                break;

            case AddressingMode.Immediate:
                HandleImmediateMode(code.Mnem, context.Size, operands, reg);
                break;

            case AddressingMode.AbsoluteIndirect:
            case AddressingMode.AbsoluteIndirectLong:
            case AddressingMode.AbsoluteIndexedIndirect:
            case AddressingMode.Absolute:
            case AddressingMode.AbsoluteIndexedX:
            case AddressingMode.AbsoluteIndexedY:
                HandleAbsoluteMode(code.Mnem, null, context.NextAddress, reg.DataBank, operands, reg);
                break;

            case AddressingMode.AbsoluteLong:
            case AddressingMode.AbsoluteLongIndexedX:
                HandleAbsoluteLongMode(code.Mnem, operands, reg);
                break;

            case AddressingMode.BlockMove:
                HandleBlockMoveMode(operands, context);
                break;

            case AddressingMode.DirectPage:
            case AddressingMode.DirectPageIndexedIndirectX:
            case AddressingMode.DirectPageIndexedX:
            case AddressingMode.DirectPageIndexedY:
            case AddressingMode.DirectPageIndirect:
            case AddressingMode.DirectPageIndirectIndexedY:
            case AddressingMode.DirectPageIndirectLong:
            case AddressingMode.DirectPageIndirectLongIndexedY:
                HandleDirectPageMode(operands);
                break;

            case AddressingMode.PCRelative:
                HandlePCRelativeMode(operands, context.NextAddress, reg, isLong: false);
                break;

            case AddressingMode.PCRelativeLong:
                HandlePCRelativeMode(operands, context.NextAddress, reg, isLong: true);
                break;

            case AddressingMode.StackRelative:
            case AddressingMode.StackRelativeIndirectIndexedY:
                HandleStackRelativeMode(operands);
                break;

            case AddressingMode.StackInterrupt:
                HandleStackInterruptMode(code.Mnem, operands, context);
                break;
        }

        return operands;
    }

    private void HandleImmediateMode(string mnemonic, int size, List<object> operands, Registers reg)
    {
        var operand = ReadImmediateOperand(size);
        operands.Add(operand);
        UpdateRegisterForImmediateInstruction(mnemonic, operand, reg);
    }

    private object ReadImmediateOperand(int size)
    {
        return size == 3 ? _dataReader.ReadUShort() : (object)_dataReader.ReadByte();
    }

    private void UpdateRegisterForImmediateInstruction(string mnemonic, object operand, Registers reg)
    {
        switch (mnemonic)
        {
            case "LDA":
                reg.Accumulator = CalculateRegisterValue(reg.Accumulator, operand);
                break;

            case "LDX":
                reg.XIndex = CalculateRegisterValue(reg.XIndex, operand);
                break;

            case "LDY":
                reg.YIndex = CalculateRegisterValue(reg.YIndex, operand);
                break;

            case "SEP":
            case "REP":
                UpdateStatusFlags(mnemonic, (byte)operand);
                break;
        }
    }


    private static ushort CalculateRegisterValue(ushort? currentValue, object operand)
    {
        return operand switch
        {
            ushort us => us,// 16-bit mode: use full operand value
            _ => (ushort)(((currentValue ?? 0) & 0xFF00u) | (byte)operand) // 8-bit mode: preserve high byte
        };
    }

    private void UpdateStatusFlags(string mnemonic, byte flagValue)
    {
        var flag = (StatusFlags)flagValue;
        var isSep = mnemonic == "SEP";

        if (flag.HasFlag(StatusFlags.AccumulatorMode))
        {
            _blockReader.AccumulatorFlags.TryAdd(_dataReader.Position, isSep);
        }

        if (flag.HasFlag(StatusFlags.IndexMode))
        {
            _blockReader.IndexFlags.TryAdd(_dataReader.Position, isSep);
        }
    }

    private void HandleAbsoluteLongMode(string mnemonic, List<object> operands, Registers reg)
    {
        var refLoc = _dataReader.ReadUShort();
        var bank = _dataReader.ReadByte();
        var address = new Address(bank, refLoc);

        if (address.Space == AddressSpace.ROM)
        {
            var wrapper = new LocationWrapper((int)address, AddressType.Address);
            if (IsJumpInstruction(mnemonic))
            {
                _blockReader.NoteType(wrapper.Location, "Code", false, reg);
            }
            operands.Add(wrapper);
        }
        else
        {
            operands.Add(address);
        }
    }

    private void HandleBlockMoveMode(List<object> operands, AsmReader.OperationContext context)
    {
        operands.Add(_dataReader.ReadByte());
        context.XForm2 = _transformProcessor.GetTransform();
        operands.Add(_dataReader.ReadByte());
    }

    private void HandleDirectPageMode(List<object> operands)
    {
        operands.Add(_dataReader.ReadByte());
    }

    private void HandlePCRelativeMode(List<object> operands, int nextAddress, Registers reg, bool isLong)
    {
        int relative = isLong
            ? nextAddress + _dataReader.ReadShort()
            : nextAddress + _dataReader.ReadSByte();

        _blockReader.NoteType(relative, "Code", reg: reg);
        operands.Add(relative);
    }

    private void HandleStackRelativeMode(List<object> operands)
    {
        operands.Add(_dataReader.ReadByte());
    }

    private void HandleStackInterruptMode(string mnemonic, List<object> operands, AsmReader.OperationContext context)
    {
        var cmd = _dataReader.ReadByte();
        operands.Add(cmd);

        if (mnemonic == "COP")
        {
            if (!_blockReader._root.CopDef.TryGetValue(cmd, out var copDef))
                throw new InvalidOperationException("Unknown COP command");

            context.CopDef = copDef;
            _copProcessor.ParseCopCommand(copDef, operands);
        }
    }

    private void HandleStackOrImpliedMode(string mnemonic, Registers reg)
    {
        var stackOperations = new StackOperations(reg, _blockReader);
        stackOperations.HandleStackOperation(mnemonic);
    }

    private void HandleAbsoluteMode(string mnemonic, byte? xBank1, int next, byte? dataBank, List<object> operands, Registers registers)
    {
        var refLoc = _dataReader.ReadUShort();

        var isPush = IsPushInstruction(mnemonic);
        if (isPush)
            refLoc++;

        var isJump = isPush || IsJumpInstruction(mnemonic);
        var bank = xBank1 ?? (isJump ? (byte)(_dataReader.Position >> 16) : dataBank ?? 0x81);

        var addr = new Address(bank, refLoc);
        if (addr.IsROM)
        {
            var wrapper = new LocationWrapper((int)addr, AddressType.Offset);
            if (isJump)
            {
                var name = _blockReader.NoteType(wrapper.Location, "Code", isPush, registers);

                if (isPush)
                {
                    operands.Add($"&{name}-1");
                    return;
                }
            }
            operands.Add(wrapper);
        }
        else
        {
            operands.Add(addr);
        }
    }

    private static bool IsJumpInstruction(string mnemonic) => mnemonic[0] == 'J';
    private static bool IsPushInstruction(string mnemonic) => mnemonic[0] == 'P';
}