using Microsoft.AspNetCore.Http;
using Taskify.Services.DTOs;

namespace Taskify.Services.Interface
{
    public interface IProfileService
    {
        Task<ApiResponse<string>> UploadProfileImageAsync(IFormFile file);
        Task<ApiResponse<string>> DeleteProfileImageAsync();
    }
}
