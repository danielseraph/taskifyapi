using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
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
            var user = _contextAccessor.HttpContext?.User;
            if (user == null) return null;

            // Try multiple common claim types (NameIdentifier, JWT sub, id, userId)
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? user.FindFirst("id")?.Value
                ?? user.FindFirst("userId")?.Value;
        }

        public string? GetUserName()
        {
            var user = _contextAccessor.HttpContext?.User;
            if (user == null) return null;

            // Prefer Identity.Name then common claim keys
            return user.Identity?.Name
                ?? user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                ?? user.FindFirst("name")?.Value;
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
            if (user == null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            return new CurrentUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Role = roles
            };
        }
    }
}