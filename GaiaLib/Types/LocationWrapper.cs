using GaiaLib.Enum;

namespace GaiaLib.Types
{
    public class LocationWrapper(int location, AddressType type)
    {
        public int Location = location;
        public AddressType Type = type;
    }
}
