using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Parses assembly instructions from ROM data, handling different addressing modes
/// and maintaining CPU register state during analysis.
/// </summary>
public class AsmReader
{
    // Constants
    private const int AccumulatorOpMask = 0xF;
    private const int AccumulatorOpValue = 0x9;
    private const int VariableSizeIndicator = -2;
    private const int TwoBytesSize = 2;
    private const int ThreeBytesSize = 3;

    private readonly BlockReader _romReader;
    private readonly TransformProcessor _transformProcessor;
    private readonly AddressingModeHandler _addressingModeHandler;
    private readonly RegisterManager _registerManager;

    public AsmReader(BlockReader romReader)
    {
        _romReader = romReader;
        _transformProcessor = new TransformProcessor(romReader);
        _registerManager = new RegisterManager(romReader);
        _addressingModeHandler = new AddressingModeHandler(romReader, _transformProcessor);
    }



    internal Op ParseAsm(Registers reg)
    {
        var opStart = _romReader._romPosition;
        var opCode = _romReader.ReadByte();
        
        if (!_romReader._root.OpCodes.TryGetValue(opCode, out var code))
            throw new InvalidOperationException("Unknown OpCode");

        var operationContext = InitializeOperation(code, reg, opStart);
        var operands = _addressingModeHandler.ProcessAddressingMode(code, operationContext, reg);
        
        _transformProcessor.ApplyTransforms(operationContext.XForm1, operationContext.XForm2, operands);

        return new Op
        {
            Location = opStart,
            Code = code,
            Size = (byte)(_romReader._romPosition - opStart),
            Operands = [.. operands],
            CopDef = operationContext.CopDef,
        };
    }

    private OperationContext InitializeOperation(OpCode code, Registers reg, int loc)
    {
        var size = CalculateInstructionSize(code, reg);
        var next = loc + size;

        _registerManager.ClearDestinationRegister(code.Mnem, reg);

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










