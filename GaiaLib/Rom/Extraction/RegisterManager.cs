using GaiaLib.Asm;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Manages CPU register operations and state
/// </summary>
public class RegisterManager
{
    private readonly BlockReader _romReader;

    public RegisterManager(BlockReader romReader)
    {
        _romReader = romReader;
    }

    public void ClearDestinationRegister(string mnemonic, Registers reg)
    {
        switch (mnemonic)
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
} 