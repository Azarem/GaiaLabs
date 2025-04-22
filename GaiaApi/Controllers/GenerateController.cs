using Force.Crc32;
using GaiaApi.ViewModels;
using GaiaLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace GaiaApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenerateController : ControllerBase
    {
        private const int RomSize = 2097152;
        private const uint RomCrc = 473450688;

        private readonly GaiaApiSettings _settings;

        public GenerateController(IOptions<GaiaApiSettings> settings)
        {
            _settings = settings.Value;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] GenerateViewModel vm)
        {
            if (vm.RomFile.Length != RomSize)
                return BadRequest();

            var isCustomBuild = !string.IsNullOrWhiteSpace(vm.Notepad) || vm.PatchFiles?.Any() == true;

            var romBytes = new byte[RomSize];
            using (var romStream = vm.RomFile.OpenReadStream())
                await romStream.ReadAsync(romBytes);

            var crc = Crc32Algorithm.Compute(romBytes);
            if (crc != RomCrc)
                return BadRequest();

            var patchName = $"iog_rxlt_{_settings.Version.Replace('.', '_')}";
            FileInfo? storeInfo = null;

            if (!isCustomBuild)
            {
                //Check store for existing patch
                var hashStream = new MemoryStream();
                hashStream.Write(Encoding.ASCII.GetBytes(patchName));

                foreach (var module in vm.Modules)
                    if (!string.IsNullOrWhiteSpace(module))
                        hashStream.Write(Encoding.ASCII.GetBytes($"|{module.Trim()}"));

                hashStream.Position = 0;
                var hash = await SHA256.HashDataAsync(hashStream);

                var key = Convert.ToHexString(hash);

                storeInfo = new FileInfo(Path.Combine(_settings.StorePath, $"{key}.smc"));
                if (storeInfo.Exists)
                {
                    var patchBytes = new byte[storeInfo.Length];

                    using (var patchFile = storeInfo.OpenRead())
                        await patchFile.ReadAsync(patchBytes, 0, patchBytes.Length);

                    return File(patchBytes, "application/octet-stream", $"{patchName}.smc");
                }
            }

            var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            while (System.IO.File.Exists(tempPath) || Directory.Exists(tempPath))
                tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var tempDir = Directory.CreateDirectory(tempPath);
            try
            {
                void copyDir(DirectoryInfo src, DirectoryInfo dst)
                {
                    if (!src.Exists)
                        return;

                    if (!dst.Exists)
                        dst.Create();

                    foreach (var item in src.GetFiles())
                        item.CopyTo(Path.Combine(dst.FullName, item.Name), true);

                    foreach (var item in src.GetDirectories())
                        copyDir(item, dst.CreateSubdirectory(item.Name));
                }

                void copyModule(string name)
                {
                    copyDir(new DirectoryInfo(Path.Combine(_settings.ModulePath, name)), tempDir);
                }

                copyModule("base");

                foreach (var module in vm.Modules)
                    if (!string.IsNullOrWhiteSpace(module))
                        copyModule(module.Trim());

                ///TODO: Process ad-hoc patches
                ///
                if (vm.PatchFiles != null)
                    foreach (var patch in vm.PatchFiles)
                    {
                        if (patch.FileName.EndsWith(".asm"))
                        {
                            using var asmStream = patch.OpenReadStream();
                            using var asmFile = System.IO.File.Create(Path.Combine(tempPath, "patches", patch.FileName));
                            await asmStream.CopyToAsync(asmFile);
                        }
                        else if (patch.FileName.EndsWith(".zip"))
                        {
                            using var zipStream = patch.OpenReadStream();
                            ZipFile.ExtractToDirectory(zipStream, tempPath, true);
                        }
                    }

                if (!string.IsNullOrWhiteSpace(vm.Notepad))
                {
                    using var npFile = System.IO.File.Create(Path.Combine(tempPath, "patches", "zz_notepad.asm"));
                    npFile.Write(Encoding.UTF8.GetBytes(vm.Notepad));
                }

                var project = new ProjectRoot()
                {
                    Name = patchName,
                    BaseDir = tempPath,
                    RomPath = _settings.RomPath,
                    DatabasePath = Path.Combine(_settings.ModulePath, "database.json"),
                    //FlipsPath = _settings.FlipsPath
                };

                project.Build();

                var patchInfo = new FileInfo(Path.Combine(tempPath, $"{project.Name}.smc"));

                if (!patchInfo.Exists)
                    return Problem("Patch file was not created");

                if (storeInfo != null)
                    patchInfo.CopyTo(storeInfo.FullName, true);

                var patchBytes = new byte[patchInfo.Length];

                using (var patchFile = patchInfo.OpenRead())
                    await patchFile.ReadAsync(patchBytes);

                return File(patchBytes, "application/octet-stream", patchInfo.Name);
            }
            finally
            {
                tempDir.Delete(true);
            }
        }
    }
}
