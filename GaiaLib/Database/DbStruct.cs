namespace GaiaLib.Database
{
    public class DbStruct
    {
        public string Name { get; set; }
        //public string[] Parts { get; set; }
        public string[] Types { get; set; }
        public string Parent { get; set; }
        public HexString? Delimiter { get; set; }

        public HexString? Descriminator { get; set; }
    }
}
