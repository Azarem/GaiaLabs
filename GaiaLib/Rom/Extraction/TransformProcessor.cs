using GaiaLib.Database;
using GaiaLib.Types;
using System.Globalization;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Handles transform processing for assembly instructions
/// </summary>
public class TransformProcessor
{
    private readonly BlockReader _romReader;

    public TransformProcessor(BlockReader romReader)
    {
        _romReader = romReader;
    }

    /// <summary>
    /// Retrieves transform information for the current ROM position
    /// </summary>
    public string? GetTransform()
    {
        if (!_romReader._root.Transforms.TryGetValue(_romReader._romPosition, out var transform) || transform == "")
            return transform;

        var transformName = CleanTransformName(transform);
        var referenceLocation = ResolveTransformReference(transformName);
        
        if (referenceLocation != null)
            _romReader.ResolveInclude(referenceLocation.Value, false);

        return transform;
    }

    /// <summary>
    /// Applies transforms to operands
    /// </summary>
    public void ApplyTransforms(string? xForm1, string? xForm2, List<object> operands)
    {
        ApplyTransform(xForm1, 0, operands);
        ApplyTransform(xForm2, 1, operands);
    }

    private void ApplyTransform(string? xform, int operandIndex, List<object> operands)
    {
        if (xform == null || operandIndex >= operands.Count)
            return;

        if (xform == "")
        {
            ApplyDefaultTransform(operandIndex, operands);
        }
        else
        {
            operands[operandIndex] = xform;
        }
    }

    private void ApplyDefaultTransform(int operandIndex, List<object> operands)
    {
        int value = _romReader._romPosition & 0xFF0000 | (ushort)operands[operandIndex];
        
        if (!_romReader._referenceTable.TryGetValue(value, out var referenceName))
        {
            referenceName = $"loc_{value:X6}";
            _romReader._referenceTable[value] = referenceName;
        }
        
        _romReader.ResolveInclude(value, false);
        operands[operandIndex] = $"&{referenceName}";
    }

    private string CleanTransformName(string transform)
    {
        var name = transform.TrimStart(RomProcessingConstants.AddressSpace);
        
        var mathIndex = name.IndexOfAny(RomProcessingConstants.Operators);
        if (mathIndex > 0)
            name = name[..mathIndex];
            
        return name;
    }

    private int? ResolveTransformReference(string transformName)
    {
        // First check reference table
        foreach (var entry in _romReader._referenceTable)
        {
            if (entry.Value == transformName)
                return entry.Key;
        }

        // Then check for location pattern
        var match = BlockReader.LocationRegex().Match(transformName);
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
        }

        return null;
    }
} 