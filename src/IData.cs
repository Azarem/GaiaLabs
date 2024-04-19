
namespace GaiaLabs
{
    public interface IData
    {
        Location Location { get; set; }
        uint Size { get; set; }
        string Label { get; set; }

        bool Unpack(RomLoader loader);
    }
}
