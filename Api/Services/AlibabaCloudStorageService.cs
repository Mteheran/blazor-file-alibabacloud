using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using shared;
using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace api.Services
{
    public class AlibabaCloudStorageService : IAlibabaCloudStorageService
    {       
        private readonly IConfiguration Configuration;
        private readonly  string bucketName = "mteheranst1";
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
}