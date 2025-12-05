using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Taskify.Services.Interface;
using Taskify.Services.Utilities;

namespace Taskify.Services.Implementation
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(IOptions<CloudinarySettings> settings)
        {
            var account = new Account(
                settings.Value.CloudName,
                settings.Value.ApiKey,
                settings.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);

        }

        public Task<ImageUploadResult> AddImage(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();
            try
            {
                if (file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Transformation = new Transformation().Crop("fill").Width(500).Height(500).Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
                return Task.FromResult(uploadResult);

            }
            catch (Exception ex)
            {
                throw new Exception("Image upload failed", ex);
            }
        }
        public Task<DeletionResult> DeleteImage(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = _cloudinary.Destroy(deleteParams);
            return Task.FromResult(result);
        }
    }
}
