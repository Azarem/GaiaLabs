
namespace GaiaLib.Database
{
    public class DbTransform
    {
        public string Block { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Transforms { get; set; }
    }
}
