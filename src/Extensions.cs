
namespace GaiaLabs
{
    public static class Extensions
    {
        public static string GetExtension(this string filePath)
        {
            if (filePath != null)
            {
                var ix = filePath.LastIndexOf('.');
                return (ix > 0 && ix < filePath.Length - 1) ? filePath[(ix + 1)..].ToLower() : null;
            }
            return null;
        }
    }
}
