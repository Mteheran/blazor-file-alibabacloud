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
        private readonly IAlibabaCloudStorageService _azureFile;

        public FileController(ILogger<FileController> logger, IAlibabaCloudStorageService azure)
        {
            _logger = logger;
            _azureFile = azure;
        }

        [HttpGet]
        public async Task<IEnumerable<BlazorFile>> Get()
        {
            _logger.LogDebug("Gettings files...");
            return await _azureFile.GetFiles();
        }

        [HttpGet("{id}")]
        public async Task<BlazorFile> Get(string id)
        {
            _logger.LogDebug("Gettings files...");
            return  await _azureFile.GetInfoFile(id);
        }

        [HttpPost]
        public IActionResult Post([FromBody] BlazorFile file)
        {
            _logger.LogDebug("Saving file...");
            _azureFile.SaveFileAsync(file);

            _logger.LogDebug("File saved!");

            return Ok();
        }
    }
}
