using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Parses assembly instructions from ROM data, handling different addressing modes
/// and maintaining CPU register state during analysis.
/// </summary>
internal class AsmReader
{
    // Constants
    private const int AccumulatorOpMask = 0xF;
    private const int AccumulatorOpValue = 0x9;
    private const int VariableSizeIndicator = -2;
    private const int TwoBytesSize = 2;
    private const int ThreeBytesSize = 3;

    private readonly BlockReader _blockReader;
    private readonly TransformProcessor _transformProcessor;
    private readonly AddressingModeHandler _addressingModeHandler;
    private readonly RomDataReader _romDataReader;

    public AsmReader(BlockReader blockReader)
    {
        _blockReader = blockReader;
        _transformProcessor = new TransformProcessor(blockReader);
        _addressingModeHandler = new AddressingModeHandler(blockReader, _transformProcessor);
        _romDataReader = blockReader._romDataReader;
    }



    internal Op ParseAsm(Registers reg)
    {
        var opStart = _romDataReader.Position;
        var opCode = _romDataReader.ReadByte();
        
        //Find matching OpCode in the database
        if (!_blockReader._root.OpCodes.TryGetValue(opCode, out var code))
            throw new InvalidOperationException("Unknown OpCode");

        //Initialize operation context
        var operationContext = InitializeOperation(code, reg, opStart);

        //Process operands based on the addressing mode
        var operands = _addressingModeHandler.ProcessAddressingMode(code, operationContext, reg);
        
        //Apply label transforms to operands after processing. This ensures that the ROM state is processed and valid.
        _transformProcessor.ApplyTransforms(operationContext.XForm1, operationContext.XForm2, operands);

        return new Op
        {
            Location = opStart,
            Code = code,
            Size = (byte)(_romDataReader.Position - opStart),
            Operands = [.. operands],
            CopDef = operationContext.CopDef,
        };
    }


    public void ClearDestinationRegister(OpCode code, Registers reg)
    {
        switch (code.Mnem)
        {
            case "LDA":
                reg.Accumulator = null;
                break;
            case "LDX":
                reg.XIndex = null;
                break;
            case "LDY":
                reg.YIndex = null;
                break;
        }
    }

    private OperationContext InitializeOperation(OpCode code, Registers reg, int loc)
    {
        //Calculate instruction size (some are variable)
        var size = CalculateInstructionSize(code, reg);

        //Calculate next address (for branching)
        var next = loc + size;

        //Clear destination register for load operations. This resets the register state
        ClearDestinationRegister(code, reg);

        return new OperationContext
        {
            Size = size,
            NextAddress = next,
            XForm1 = _transformProcessor.GetTransform(),
            XForm2 = null,
            CopDef = null
        };
    }

    private int CalculateInstructionSize(OpCode code, Registers reg)
    {
        var size = code.Size;
        
        if (size == VariableSizeIndicator)
        {
            if ((code.Code & AccumulatorOpMask) == AccumulatorOpValue)
            {
                size = reg.AccumulatorFlag ?? false ? TwoBytesSize : ThreeBytesSize;
            }
            else
            {
                size = reg.IndexFlag ?? false ? TwoBytesSize : ThreeBytesSize;
            }
        }
        
        return size;
    }

    /// <summary>
    /// Contains context information for an operation being processed
    /// </summary>
    public class OperationContext
    {
        public int Size { get; set; }
        public int NextAddress { get; set; }
        public string? XForm1 { get; set; }
        public string? XForm2 { get; set; }
        public CopDef? CopDef { get; set; }
    }
}










