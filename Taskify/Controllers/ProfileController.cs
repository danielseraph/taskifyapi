using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskify.Services.Interface;

namespace Taskify.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService        ;

        public ProfileController(IProfileService profileService)
        {

            _profileService = profileService;
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            var result = await _profileService.UploadProfileImageAsync(file);
            return StatusCode(result.StatusCode, result);
        }


        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProfileImage()
        {
            var result = await _profileService.DeleteProfileImageAsync();
            return StatusCode(result.StatusCode, result);
        }
    }
}
