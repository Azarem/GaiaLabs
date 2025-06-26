using GaiaLib.Types;

namespace GaiaLib.Database
{
    public class DbStruct
    {
        public string Name { get; set; }
        //public string[] Parts { get; set; }
        public string[] Types { get; set; }
        public string Parent { get; set; }
        public int? Delimiter { get; set; }

        public int? Discriminator { get; set; }
    }
}
