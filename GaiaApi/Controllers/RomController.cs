using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace GaiaApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RomController : ControllerBase
    {
        private readonly GaiaApiSettings _settings;

        public RomController(IOptions<GaiaApiSettings> settings)
        {
            _settings = settings.Value;
        }

        [HttpGet("{**path}")]
        public IActionResult GetRom(string path)
        {
            if (Request.Headers["Auth-Token"] != _settings.AuthToken)
                return Unauthorized();

            var filePath = Path.Combine(_settings.RomPath, path);

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}