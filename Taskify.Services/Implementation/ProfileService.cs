using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;

namespace Taskify.Services.Implementation
{
    public class ProfileService : IProfileService
    {
        private readonly IImageService _imageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<AppUser> _userManager;

        public ProfileService(IImageService imageService, ICurrentUserService currentUserService, UserManager<AppUser> userManager)
        {
            _imageService = imageService;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<ApiResponse<string>> UploadProfileImageAsync(IFormFile file)
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponseBuilder.Fail<string>("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseBuilder.Fail<string>("User not found", statusCode: StatusCodes.Status404NotFound);

            if (file == null || file.Length == 0)
                return ApiResponseBuilder.Fail<string>("No file provided", statusCode: StatusCodes.Status400BadRequest);

            var uploadResult = await _imageService.AddImage(file);
            if (uploadResult == null || uploadResult.Error != null || string.IsNullOrWhiteSpace(uploadResult.SecureUrl?.ToString()))
                {
                var err = uploadResult?.Error?.Message ?? "Image upload failed";
                return ApiResponseBuilder.Fail<string>(err, statusCode: StatusCodes.Status500InternalServerError);
            }

            user.ProfileImageUrl = uploadResult.SecureUrl.ToString();
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ApiResponseBuilder.Fail<string>("Failed to update user profile with image", statusCode: StatusCodes.Status500InternalServerError);

            return ApiResponseBuilder.Success(user.ProfileImageUrl, statusCode: StatusCodes.Status200OK);
        }

        public async Task<ApiResponse<string>> DeleteProfileImageAsync()
        {
            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return ApiResponseBuilder.Fail<string>("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseBuilder.Fail<string>("User not found", statusCode: StatusCodes.Status404NotFound);

            if (string.IsNullOrWhiteSpace(user.ProfileImageUrl))
                return ApiResponseBuilder.Fail<string>("No profile image to delete", statusCode: StatusCodes.Status404NotFound);

            // Note: public id is not stored — remote deletion not attempted.
            var removedUrl = user.ProfileImageUrl;
            user.ProfileImageUrl = null;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return ApiResponseBuilder.Fail<string>("Failed to remove profile image", statusCode: StatusCodes.Status500InternalServerError);

            return ApiResponseBuilder.Success(removedUrl!, "Profile image removed successfully", statusCode: StatusCodes.Status200OK);
        }
    }
}
