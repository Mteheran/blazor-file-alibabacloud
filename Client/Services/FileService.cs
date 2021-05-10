using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace client.Services
{
    public class FileService : IFileService
    {
        IJSRuntime jsRuntime {get;set;}
        public FileService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;

        }

        public async Task DownloadBinaryOptim(string fileName, byte[] fileInfo)
        {
            string contentType = "application/octet-stream";

            // Check if the IJSRuntime is the WebAssembly implementation of the JSRuntime
            if (jsRuntime is IJSUnmarshalledRuntime webAssemblyJSRuntime)
            {
                webAssemblyJSRuntime.InvokeUnmarshalled<string, string, byte[], bool>("BlazorDownloadFileFast", fileName, contentType, fileInfo);
            }
            else
            {
                // Fall back to the slow method if not in WebAssembly
                await jsRuntime.InvokeVoidAsync("BlazorDownloadFile", fileName, contentType, fileInfo);
            }
        }
    }

    public interface IFileService
    {
        Task DownloadBinaryOptim(string fileName, byte[] fileInfo);
    }
}