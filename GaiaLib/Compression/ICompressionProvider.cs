using GaiaLib.Types;

namespace GaiaLib.Compression
{
    public interface ICompressionProvider
    {
        byte[] Expand(byte[] srcData, int srcPosition = 0, int srcLen = RomProcessingConstants.PageSize);
        byte[] Compact(byte[] srcData);
    }
}
