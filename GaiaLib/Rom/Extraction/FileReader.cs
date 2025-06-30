using GaiaLib.Compression;
using GaiaLib.Database;
using GaiaLib.Enum;

namespace GaiaLib.Rom.Extraction;

internal class FileReader(byte[] romData, DbRoot dbRoot, ICompressionProvider provider)
{
    public const int PaletteMinSize = 0x200;

    private readonly byte[] _romData = romData;
    private readonly DbRoot _dbRoot = dbRoot;
    private readonly ICompressionProvider _compression = provider;

    /// <summary>
    /// Extracts all files from the ROM to the given output path
    /// </summary>
    /// <param name="outPath">The path to extract the files to</param>
    public async Task Extract(string outPath)
    {
        //Process each file in the repository
        foreach (var file in _dbRoot.Files)
        {
            var start = file.Start;

            //Get the path options for the resource type
            var res = _dbRoot.GetPath(file.Type);

            var filePath = outPath;

            //If resource has a folder, append it to the output path and create the directory
            if (!string.IsNullOrWhiteSpace(res.Folder))
            {
                filePath = Path.Combine(outPath, res.Folder);
                Directory.CreateDirectory(filePath);
            }

            //Append file name and extension to the output path
            filePath = Path.Combine(filePath, $"{file.Name}.{res.Extension}");

            //If the file already exists, skip it
            if (File.Exists(filePath))
                continue;

            //Store file header information for post-processing
            byte[]? header;
            if (file.Type == BinType.Tilemap) //Tilemap files have a header of 2 bytes
                header = [_romData[start++], _romData[start++]];
            else if (file.Type == BinType.Meta17) //Meta17 files have a header of 4 bytes
                header = [_romData[start++], _romData[start++], _romData[start++], _romData[start++]];
            else
                header = null;

            //Get the length of the raw file
            var length = file.End - start;

            //Create the file stream
            using var fileStream = File.Create(filePath);

            //If the file is compressed, expand it
            if (file.Compressed == true)
            {
                //Expand the compressed data
                var data = _compression.Expand(_romData, start, length);

                //Copy the expanded data to the file stream
                await CopyBytes(fileStream, data, 0, data.Length, header, file.Type);
            }
            else
            {
                //When "Compressed" is not null (and implied false), this means there is a zero compression header. Skip it
                if (file.Compressed != null)
                {
                    start += 2;
                    length -= 2;
                }

                //Copy the uncompressed data from the ROM to the file stream
                await CopyBytes(fileStream, _romData, start, length, header, file.Type);
            }
        }
    }

    private static async Task CopyBytes(Stream outStream, byte[] data, int position, int length, byte[]? header, BinType type)
    {
        //Write the header if it exists
        if (header != null)
            await outStream.WriteAsync(header);

        //Write the data using provided position and length
        await outStream.WriteAsync(data.AsMemory(position, length));

        //Expand palette to 16 sets so it is compatible with yy-chr
        if (type == BinType.Palette)
        {
            for (int remain = PaletteMinSize - length; remain > 0; remain--)
                outStream.WriteByte(0);
        }
    }
}
