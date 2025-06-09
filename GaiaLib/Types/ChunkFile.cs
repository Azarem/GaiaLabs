using GaiaLib.Asm;
using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Types;

public class ChunkFile
{
    public string Path;
    public string Name;
    public int Size;
    public int Location;
    public List<AsmBlock>? Blocks;
    public HashSet<string>? Includes;
    public Dictionary<string, AsmBlock> IncludeLookup;
    public byte? Bank;
    public BinType Type;
    public bool? Compressed;
    public bool Upper;

    public ChunkFile(string path, BinType type)
    {
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(path);
        Type = type;
        Size = (int)new FileInfo(path).Length;
        IncludeLookup = new Dictionary<string, AsmBlock>();

        Compressed = type switch
        {
            BinType.Bitmap
            or BinType.Tilemap
            or BinType.Tileset
            or BinType.Spritemap
            or BinType.Meta17 => false,
            _ => null,
        };
    }

    public void Rebase()
    {
        if (Blocks == null)
            return;

        var loc = Location;
        for (int x = 0, len = Blocks.Count; x < len; x++)
        {
            var block = Blocks[x];
            if (x > 0 && block.Label == null)
                break;
            block.Location = loc;
            loc += block.Size;
        }
    }

    public void Rebase(int loc)
    {
        Location = loc;
        Rebase();
    }

    public int CalculateSize()
    {
        if (Blocks != null)
            Size = CalculateSize(Blocks);
        return Size;
    }

    public static int CalculateSize(List<AsmBlock> blocks)
    {
        int size = 0;
        for (int x = 0, len = blocks.Count; x < len; x++)
        {
            var block = blocks[x];
            if (x > 0 && block.Label == null)
                break;
            size += block.Size;
        }
        return size;
    }
} 