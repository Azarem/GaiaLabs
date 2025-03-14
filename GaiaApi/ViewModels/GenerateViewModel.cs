namespace GaiaApi.ViewModels
{
    public class GenerateViewModel
    {
        public IFormFile RomFile { get; set; }
        public List<string> Modules { get; set; }
        public string? Notepad { get; set; }
        public IFormFileCollection? PatchFiles { get; set; }
    }
}
