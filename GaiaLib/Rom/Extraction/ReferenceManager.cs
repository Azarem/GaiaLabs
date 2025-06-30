using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;
using System.Globalization;

namespace GaiaLib.Rom.Extraction;

/// <summary>
/// Manages references, chunks, and markers during ROM analysis
/// </summary>
internal class ReferenceManager
{
    internal readonly Dictionary<int, string> _structTable = [];
    internal readonly Dictionary<int, int> _markerTable = [];
    internal readonly Dictionary<int, string> _nameTable = [];
    private readonly DbRoot _root;

    public ReferenceManager(DbRoot root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public bool TryGetStruct(int location, out string? chunkType)
    {
        return _structTable.TryGetValue(location, out chunkType);
    }

    public bool TryAddStruct(int location, string chunkType)
    {
        return _structTable.TryAdd(location, chunkType);
    }

    public bool ContainsStruct(int location)
    {
        return _structTable.ContainsKey(location);
    }

    public bool TryGetName(int location, out string? referenceName)
    {
        return _nameTable.TryGetValue(location, out referenceName);
    }

    public bool TryAddName(int location, string referenceName)
    {
        return _nameTable.TryAdd(location, referenceName);
    }

    public bool TryGetMarker(int location, out int offset)
    {
        return _markerTable.TryGetValue(location, out offset);
    }

    public void SetMarker(int location, int offset)
    {
        _markerTable[location] = offset;
    }

    public string CreateBranchLabel(int location)
    {
        var name = string.Format(RomProcessingConstants.BlockReader.LocationFormat, location);
        _nameTable[location] = name;
        return name;
    }

    public string CreateTypeName(string type, int location)
    {
        var name = type.ToLower();
        while (RomProcessingConstants.BlockReader.PointerCharacters.Contains(name[0]))
            name = name[1..] + "_list";

        var result = string.Format(RomProcessingConstants.BlockReader.TypeNameFormat, name, location);
        return result;
    }

    public string CreateFallbackName(int location)
    {
        var fileMatch = _root.Files.FirstOrDefault(x =>
            x.Start <= location && x.End > location);

        if (fileMatch != null)
        {
            var offset = location - fileMatch.Start;
            return fileMatch.Name + (offset != 0 ? string.Format(RomProcessingConstants.BlockReader.OffsetFormat, offset) : "");
        }

        return location.ToString("X6");
    }

    public string ResolveName(int location, AddressType type, bool isBranch)
    {
        var prefix = Address.CodeFromType(type);
        string? name;
        string? label = null;

        // Handle rewrites first
        if (_root.Rewrites.TryGetValue(location, out int rewrite))
        {
            (location, label) = ProcessRewrite(location, rewrite);
        }

        // Try to get existing reference
        if (!_nameTable.TryGetValue(location, out name))
        {
            name = isBranch ? 
                CreateBranchLabel(location) : 
                FindClosestReference(location) ?? CreateFallbackName(location);
        }

        return $"{prefix}{name}{label}";
    }

    public string? FindClosestReference(int location)
    {
        int closestDistance = RomProcessingConstants.BlockReader.RefSearchMaxRange;
        string? closestName = null;
        int? closestLocation = null;

        foreach (var entry in _nameTable)
        {
            if (entry.Key > location)
                continue;

            var distance = location - entry.Key;
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestName = entry.Value;
            closestLocation = entry.Key;

            if (closestDistance == 1)
                break;
        }

        return ProcessClosestMatch(location, closestName, closestLocation, closestDistance);
    }


    private (int location, string? label) ProcessRewrite(int location, int rewrite)
    {
        var offset = location - rewrite;
        char cmd = offset < 0 ? '-' : '+';
        if (offset < 0) offset = -offset;

        string? label = null;
        if (_structTable.TryGetValue(rewrite, out var ctype) && ctype == RomProcessingConstants.BlockReader.WideStringType)
        {
            _markerTable[rewrite] = offset;
            _markerTable[location] = offset;
            label = cmd == '-' ? RomProcessingConstants.BlockReader.NegativeMarkerFormat : RomProcessingConstants.BlockReader.MarkerFormat;
        }
        else
        {
            var formatString = cmd == '-' ? RomProcessingConstants.BlockReader.NegativeOffsetFormat : RomProcessingConstants.BlockReader.OffsetFormat;
            label = string.Format(formatString, offset);
        }

        return (rewrite, label);
    }

    private string? ProcessClosestMatch(int location, string? closestName, int? closestLocation, int closestDistance)
    {
        if (closestName == null)
            return null;

        string result = closestName;

        if (_structTable.TryGetValue(closestLocation!.Value, out var ctype) && ctype == RomProcessingConstants.BlockReader.WideStringType)
        {
            _markerTable[closestLocation.Value] = closestDistance;
            _markerTable[location] = closestDistance;
            result += RomProcessingConstants.BlockReader.MarkerFormat;
        }
        else
        {
            result += string.Format(RomProcessingConstants.BlockReader.OffsetFormat, closestDistance);
        }

        return result;
    }
} 