using GaiaLib.Database;
using GaiaLib.Enum;
using GaiaLib.Types;

namespace GaiaLib.Rom.Extraction;

internal class SfxReader
{
    private readonly byte[] _romData;
    private readonly DbRoot _dbRoot;
    private int _location;
    private int _count;

    public SfxReader(byte[] romData, DbRoot dbRoot)
    {
        _romData = romData;
        _dbRoot = dbRoot;
        _location = dbRoot.Config.SfxLocation;
        _count = dbRoot.Config.SfxCount;
    }

    /// <summary>
    /// Reads a byte from the rom data
    /// </summary>
    /// <returns>The byte read</returns>
    private int ReadByte()
    {
        //Only read from the lower bank. If we are in the upper bank, move to the next bank
        if ((_location & Address.UpperBank) != 0)
            _location += Address.UpperBank;

        return _romData[_location++];
    }

    /// <summary>
    /// Reads a short from the rom data
    /// </summary>
    /// <returns>The short read</returns>
    private int ReadShort()
    {
        return ReadByte() | ReadByte() << 8;
    }

    /// <summary>
    /// Extracts the sfx from the rom data to the given output path
    /// </summary>
    /// <param name="outPath">The path to extract the sfx to</param>
    public async Task Extract(string outPath)
    {
        var res = _dbRoot.GetPath(BinType.Sound);
        if (!string.IsNullOrEmpty(res?.Folder))
            outPath = Path.Combine(outPath, res.Folder);

        Directory.CreateDirectory(outPath);

        for (int i = 0; i < _count; i++)
        {
            //Read the size of the sfx
            int size = ReadShort();

            //Generate the full file path
            string filePath = Path.Combine(outPath, $"sfx{i:X2}.{res.Extension}");

            //If the file exists, skip it
            if (File.Exists(filePath))
                //Advance the read position to the next sfx
                _location += size;
            else
            {
                //Read sfx data into temporary buffer
                var sfxData = new byte[size];
                for (int x = 0; x < size; x++)
                    sfxData[x] = (byte)ReadByte();

                //Create the file and write the sfx data to it
                using var file = File.Create(filePath);
                await file.WriteAsync(sfxData.AsMemory(0, size));
            }
        }
    }
}
