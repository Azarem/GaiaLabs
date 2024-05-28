
namespace GaiaLabs
{
    public class DataReference
    {
        public byte Offset { get; set; } = 0;
        public byte Size { get; set; } = 2;

        public Location Location { get; set; }
        public DataEntry Resource { get; set; }

        public DataReference() { }

        public DataReference(Address address, byte size = 2, byte offset = 0, DataEntry resource = null)
        {
            Location = (Location)address;
            Size = size;
            Offset = offset;
            Resource = resource;
        }

        public override string ToString()
        {
            return Location.ToString();
        }
    }
}
