using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using shared;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IAlibabaCloudStorageService _alibabaCloudFile;

        public FileController(ILogger<FileController> logger, IAlibabaCloudStorageService alibabaCloud)
        {
            _logger = logger;
            _alibabaCloudFile = alibabaCloud;
        }

        [HttpGet]
        public async Task<IEnumerable<BlazorFile>> Get()
        {
            _logger.LogDebug("Gettings files...");
            return await _alibabaCloudFile.GetFiles();
        }

        [HttpGet("{id}")]
        public async Task<BlazorFile> Get(string id)
        {
            _logger.LogDebug("Gettings files...");
            return  await _alibabaCloudFile.GetInfoFile(id);
        }

        [HttpPost]
        public IActionResult Post([FromBody] BlazorFile file)
        {
            _logger.LogDebug("Saving file...");
            _alibabaCloudFile.SaveFileAsync(file);

            _logger.LogDebug("File saved!");

            return Ok();
        }
    }
}
