## Blazor file management wth Alibaba cloud
In this repository you will find a complete guide to manage file in blazor and save files in alibaba cloud OSS

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

Using "attributes" you can set HTML attributes to the control and indicate the extension file accepted

```csharp

<InputFile OnChange="@OnInputFileChange"  class="btn btn-primary" @attributes="browseattributes"  />   

Dictionary<string, object>  browseattributes = new  Dictionary<string, object>()
{
             { "accept", ".csv"} //accept online csv
};

```



You can use memory stream to read all the information from the file

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
### AlibabaCloud storage

AlibabaCloudStorageService is used to save and get information for a specifict container in AlibabaCloud object storage service

```csharp
 public class AlibabaCloudStorageService : IAlibabaCloudStorageService
    {       
        private readonly IConfiguration Configuration;
        private readonly  string bucketName = "mybucketname";
        private string connectionString = "";
        private readonly string accessKeyId = "<yourAccessKeyId>";
        private readonly string accessKeySecret = "<yourAccessKeySecret>";
        private readonly string endpoint = "http://oss-cn-hangzhou.aliyuncs.com";
        public AlibabaCloudStorageService(IConfiguration configuration)
        {
            Configuration = configuration;
            accessKeyId =  Configuration["AccessKeyId"];
            accessKeySecret =  Configuration["AccessKeySecret"];
            endpoint =  Configuration["Endpoint"];
        }
        public async Task SaveFileAsync(BlazorFile file)
        {
           // Create a ClientConfiguration instance. Modify parameters as required.
            var conf = new ClientConfiguration();

            // Enable CNAME. CNAME indicates a custom domain bound to a bucket.
            //conf.IsCname = true;

            // Create an OSSClient instance.
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);

            client.PutObject(bucketName, file.FileName, new MemoryStream(file.FileInfo));
        }

        public async Task<IEnumerable<BlazorFile>> GetFiles()
        {
            // Create a ClientConfiguration instance. Modify parameters as required.
            var conf = new ClientConfiguration();

            // Enable CNAME. CNAME indicates a custom domain bound to a bucket.
            //conf.IsCname = true;

            // Create an OSSClient instance.
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);

            ObjectListing objects = client.ListObjects(bucketName);

            List<BlazorFile> list = new List<BlazorFile>();

            foreach(var item in objects.ObjectSummaries)
            {
                var newBlazorFile = new BlazorFile() { FileName = item.Key  };
                list.Add(newBlazorFile);
            }

            return list;
        }
    
        public async Task<BlazorFile> GetInfoFile(string fileName)
        {
          // Create a ClientConfiguration instance. Modify parameters as required.
            var conf = new ClientConfiguration();

            // Enable CNAME. CNAME indicates a custom domain bound to a bucket.
            //conf.IsCname = true;

            // Create an OSSClient instance.
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);

            var objectinfo = client.GetObject(bucketName, fileName);

            MemoryStream ms = new MemoryStream();  
            await objectinfo.Content.CopyToAsync(ms);
            
            var newBlazorFile = new BlazorFile() { FileName = objectinfo.Key, FileInfo = ms.ToArray()  };

            return newBlazorFile;
        }
    
    }

    public interface IAlibabaCloudStorageService
    {
        Task SaveFileAsync(BlazorFile file);
        Task<IEnumerable<BlazorFile>> GetFiles();
        Task<BlazorFile> GetInfoFile(string fileName);
    }
```

You can set up this dependency on en Startup.cs file

```csharp
 services.AddScoped<IAlibabaCloudStorageService, AlibabaCloudStorageService>();
```

Expose the method using the FileController.cs

```csharp
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
```