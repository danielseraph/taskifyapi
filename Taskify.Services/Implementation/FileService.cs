using System;
using System.IO;
using System.Net.Http;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Taskify.Services.Interface;
using Taskify.Services.Utilities;

namespace Taskify.Services.Implementation
{
    public class FileService : IFileService
    {

        private readonly Cloudinary _cloudinary;
        private readonly IHttpClientFactory _httpClientFactory;

        public FileService(IOptions<CloudinarySettings> settings, IHttpClientFactory httpClientFactory)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            Account account = new Account(
                settings.Value.CloudName,
                settings.Value.ApiKey,
                settings.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<RawUploadResult> UploadFileAsync(IFormFile file, string folder = "documents")
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.Length == 0) throw new ArgumentException("File is empty", nameof(file));

            RawUploadResult uploadResult = new RawUploadResult();
            using Stream stream = file.OpenReadStream();

            string baseName = Path.GetFileNameWithoutExtension(file.FileName);
            string uniqueId = Guid.NewGuid().ToString("N");
            string publicId = $"{folder}/{baseName}-{uniqueId}";

            RawUploadParams uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = publicId,
                Folder = folder
            };

            uploadResult = await Task.Run(() => _cloudinary.Upload(uploadParams));
            return uploadResult;
        }

        public async Task<DeletionResult> DeleteFileAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentNullException(nameof(publicId));

            DeletionParams deletionParams = new DeletionParams(publicId)
            {
                // ResourceType is set via constructor or defaults, do not assign
            };

            // If DeletionParams does not default to Raw, use the constructor overload or method to set it
            deletionParams.Type = "raw";

            DeletionResult result = await Task.Run(() => _cloudinary.Destroy(deletionParams));
            return result;
        }

        public async Task<Stream?> DownloadFileAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentNullException(nameof(publicId));

            GetResourceParams getParams = new GetResourceParams(publicId)
            {
                // ResourceType is set via constructor or defaults, do not assign
            };

            // If GetResourceParams does not default to Raw, use the constructor overload or method to set it
            getParams.Type = "raw";

            GetResourceResult resource = await Task.Run(() => _cloudinary.GetResource(getParams));
            if (resource == null || string.IsNullOrWhiteSpace(resource.SecureUrl))
                return null;

            HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(resource.SecureUrl);
            if (!response.IsSuccessStatusCode)
                return null;

            Stream stream = await response.Content.ReadAsStreamAsync();
            return stream;
        }

        public async Task<string?> GetFileUrlAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) throw new ArgumentNullException(nameof(publicId));

            GetResourceParams getParams = new GetResourceParams(publicId)
            {
                // ResourceType is set via constructor or defaults, do not assign
            };

            // If GetResourceParams does not default to Raw, use the constructor overload or method to set it
            getParams.Type = "raw";

            GetResourceResult resource = await Task.Run(() => _cloudinary.GetResource(getParams));
            return resource?.SecureUrl;
        }
    }
}

