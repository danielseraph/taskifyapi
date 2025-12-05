using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;

namespace Taskify.Services.Interface
{
    public interface IImageService
    {
        Task<ImageUploadResult> AddImage(IFormFile file);
        Task<DeletionResult> DeleteImage(string publicId);
    }
}
