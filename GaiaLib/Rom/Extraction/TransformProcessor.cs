using GaiaLib.Database;
using GaiaLib.Types;
using System.Globalization;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Handles transform processing for assembly instructions
/// </summary>
internal class TransformProcessor
{
    private readonly BlockReader _blockReader;
    private readonly RomDataReader _romDataReader;
    private readonly ReferenceManager _referenceManager;
    private readonly IDictionary<int, string> _labelLookup;

    public TransformProcessor(BlockReader romReader)
    {
        _blockReader = romReader;
        _romDataReader = romReader._romDataReader;
        _referenceManager = romReader._referenceManager;
        _labelLookup = romReader._root.Labels;
    }

    /// <summary>
    /// Retrieves transform information for the current ROM position
    /// </summary>
    public string? GetTransform()
    {
        if (!_labelLookup.TryGetValue(_romDataReader.Position, out var transform) || transform == "")
            return transform;

        var transformName = CleanTransformName(transform);
        var referenceLocation = ResolveTransformReference(transformName);
        
        if (referenceLocation != null)
            _blockReader.ResolveInclude(referenceLocation.Value, false);

        return transform;
    }

    /// <summary>
    /// Applies transforms to operands
    /// </summary>
    public void ApplyTransforms(string? op1Label, string? op2Label, List<object> operands)
    {
        ApplyTransform(op1Label, 0, operands);
        ApplyTransform(op2Label, 1, operands);
    }

    private void ApplyTransform(string? transform, int operandIndex, List<object> operands)
    {
        if (transform == null || operandIndex >= operands.Count)
            return;

        // There is special logic for empty labels
        if (transform == "")
        {
            ApplyDefaultTransform(operandIndex, operands);
        }
        else
        {
            operands[operandIndex] = transform; //Otherwise it is a direct replacement
        }
    }

    private void ApplyDefaultTransform(int operandIndex, List<object> operands)
    {
        int value = _romDataReader.Position & 0xFF0000 | (ushort)operands[operandIndex];
        
        if (!_referenceManager.TryGetName(value, out var referenceName))
        {
            referenceName = $"loc_{value:X6}";
            _referenceManager._nameTable[value] = referenceName;
        }
        
        _blockReader.ResolveInclude(value, false);
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
        foreach (var entry in _referenceManager._nameTable)
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