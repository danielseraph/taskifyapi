using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;

namespace Taskify.Services.Implementation
{
    public class CurrentUserServie : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserManager<AppUser> _userManager;
        public CurrentUserServie(IHttpContextAccessor contextAccessor, UserManager<AppUser> userManager)
        {
            _contextAccessor = contextAccessor;
            _userManager = userManager;
        }

        public string? GetUserId()
        {
            return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetUserName()
        {
            return _contextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
        }
        public bool IsAuthenticated()
        {
            return _contextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }

        public async Task<CurrentUserDto?> GetCurrentUserAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId)) return null;
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user!);
            return new CurrentUserDto
            {
                Id = user!.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                Role = roles
            };
        }
    }
}
