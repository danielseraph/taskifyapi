using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Taskify.Services.Interface
{
    public interface IFileService
    {
        Task<RawUploadResult> UploadFileAsync(IFormFile file, string folder = "documents");
        Task<DeletionResult> DeleteFileAsync(string publicId);
        Task<Stream?> DownloadFileAsync(string publicId);
        Task<string?> GetFileUrlAsync(string publicId);
    }
}
