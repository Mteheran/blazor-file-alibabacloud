## Blazor file management
In this repository you will find a complete guide to manage file in blazor

First, we have to create a new component and add the InputFile

```csharp
<InputFile OnChange="@OnInputFileChange"  class="btn btn-primary" />   
```

You can add a multiple file selection using "multiple" tag

```csharp
<InputFile OnChange="@OnInputFileChange"  class="btn btn-primary" multiple />   
```

You must implement the OnChange method to get all the information about the file selected
```csharp
IBrowserFile file;
public async Task OnInputFileChange(InputFileChangeEventArgs e)  
{  
    file = e.File;
}
```

You can read and show the properties from the IBrowserFile

```html
<div class="form-group">
    <p>@file?.Name</p>
    <p>@file?.ContentType</p>
    <p>@(file?.Size != null ? file?.Size/1024 + " KBs" : "")</p>
    <p>@file?.LastModified</p>
</div>
```

Using Regex we can validate the file extension

```csharp
IBrowserFile file;
public async Task OnInputFileChange(InputFileChangeEventArgs e)  
{  
   
    Regex regex = new Regex(".+\\.csv", RegexOptions.Compiled);  
    if (regex.IsMatch(e.File))  
    {  
        file = e.File;
    }
}
```



Using Regex we can validate the file extension

```csharp
IBrowserFile file;
public async Task OnInputFileChange(InputFileChangeEventArgs e)  
{   
    Regex regex = new Regex(".+\\.csv", RegexOptions.Compiled);  
    if (regex.IsMatch(e.File))  
    {  
         file = e.File;
        var stream = singleFile.OpenReadStream();  
        var csv = new List<string[]>();  
        MemoryStream ms = new MemoryStream();  
        await stream.CopyToAsync(ms);  
        stream.Close();  
        var outputFileString = System.Text.Encoding.UTF8.GetString(ms.ToArray());  
  
        foreach (var item in outputFileString.Split(Environment.NewLine))  
        {  
            csv.Add(SplitCSV(item.ToString()));  
        }  
    }
}


private string[] SplitCSV(string input)  
{  
    //Excludes commas within quotes  
    Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);  
    List<string> list = new List<string>();  
    string curr = null;  
    foreach (Match match in csvSplit.Matches(input))  
    {  
        curr = match.Value;  
        if (0 == curr.Length)  list.Add(""); 
        list.Add(curr.TrimStart(','));  
    }  
  
    return list.ToArray();  
}  
```
### Azure storage

AzureStorageService is used to save and get information for a specifict container in Azure blob storage

```csharp
public class AzureStorageService : IAzureStorageService
    {       
        private readonly IConfiguration Configuration;
        private readonly  string containerName = "csvcontainer";
        private string connectionString = "";
        public AzureStorageService(IConfiguration configuration)
        {
            Configuration = configuration;
            connectionString =  Configuration["AZURE_STORAGE_CONNECTION_STRING"];
        }
        public async Task SaveFileAsync(BlazorFile file)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);    

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName); 

            await containerClient.UploadBlobAsync(file.FileName, new MemoryStream(file.FileInfo));
        }

        public async Task<IEnumerable<BlazorFile>> GetFiles()
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName); 

            var blobs = containerClient.GetBlobs(Azure.Storage.Blobs.Models.BlobTraits.All, Azure.Storage.Blobs.Models.BlobStates.Version);
            List<BlazorFile> list = new List<BlazorFile>();

            foreach(var item in blobs)
            {
                var newBlazorFile = new BlazorFile() { FileName = item.Name  };
                list.Add(newBlazorFile);
            }

            return list;
        }
    
        public async Task<BlazorFile> GetInfoFile(string fileName)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName); 

            var blobFile = containerClient.GetBlobClient(fileName);
            var fileInfoInMemory = await blobFile.DownloadAsync();

            MemoryStream ms = new MemoryStream();  

            await fileInfoInMemory.Value.Content.CopyToAsync(ms);
            
            var newBlazorFile = new BlazorFile() { FileName = blobFile.Name, FileInfo = ms.ToArray()  };

            return newBlazorFile;
        }
    
    }

    public interface IAzureStorageService
    {
        Task SaveFileAsync(BlazorFile file);
        Task<IEnumerable<BlazorFile>> GetFiles();
        Task<BlazorFile> GetInfoFile(string fileName);
    }
```

You can set up this dependency on en Startup.cs file

```csharp
 services.AddScoped<IAzureStorageService, AzureStorageService>();
```

Expose the method using the FileController.cs

```csharp
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IAzureStorageService _azureFile;

        public FileController(ILogger<FileController> logger, IAzureStorageService azure)
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
```