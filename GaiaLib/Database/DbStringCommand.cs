namespace GaiaLib.Database
{
    public class DbStringCommand
    {
        public HexString Code { get; set; }
        public string Value { get; set; }
        public MemberType[] Types { get; set; }
        public HexString? Delimiter { get; set; }
    }
}
