using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.Interface;

namespace Taskify.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICurrentUserService _currentUser;
        public UserService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUser = currentUser;
        }

        public async Task<IEnumerable<UserDto>> GetAllUserAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    Email = user.Email ?? "",
                    Roles = roles
                }); 
            }
            return result;
        }

        public async Task<CurrentUserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new CurrentUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                Role = roles
            };
        }
        public async Task<UserDto?> GetUserAsync()
        {
            var userId = _currentUser.GetUserId();
            if (string.IsNullOrWhiteSpace(userId)) return null;
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user!);
            return new UserDto
            {
                Id = user!.Id,
                profileImage = user.ProfileImageUrl!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Email = user.Email!,
                Roles = roles
            };
        }

        public async Task<ApiResponse<UserDto?>> UpdateUserProfileAsync(UpdateUserDto dto)
        {
            var currentUserId = _currentUser.GetUserId();
            if (currentUserId == null)
                throw new UnauthorizedAccessException("You are not allowed to update this profile");

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null) return ApiResponseBuilder.Fail<UserDto?>("Fail to feach user", statusCode: StatusCodes.Status400BadRequest);

            user.UserName = $"{dto.FirstName} {dto.LastName}".Trim();
            user.Email = dto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return ApiResponseBuilder.Fail<UserDto?>("Fail to update profile", statusCode: StatusCodes.Status400BadRequest);

            var roles = await _userManager.GetRolesAsync(user);

            var updatedUser = new UserDto
            {
                Id = user.Id,
                profileImage = user.ProfileImageUrl?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "",
                Roles = roles
            };
            return ApiResponseBuilder.Success(updatedUser, "Profile updated successfully");
        }

        public async Task<ApiResponse<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null || !users.Any())
                return ApiResponseBuilder.Fail<IEnumerable<UserDto>>("No user found", statusCode: StatusCodes.Status404NotFound);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName!,
                FirstName = u.FirstName!,
                LastName = u.LastName!,
                Email = u.Email!,
            }).ToList();

            return ApiResponseBuilder.Success<IEnumerable<UserDto>>(
                userDtos,
                "Users retrieved successfully",
                statusCode: StatusCodes.Status200OK
            );
        }
    }
}
