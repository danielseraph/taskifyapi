using Taskify.Services.DTOs;

namespace Taskify.Services.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUserAsync();
        Task<CurrentUserDto?> GetUserByIdAsync(string id);
        Task<UserDto?> GetUserAsync();
        Task<ApiResponse<UserDto?>> UpdateUserProfileAsync(UpdateUserDto dto);
        Task<ApiResponse<IEnumerable<UserDto>>> GetUsers();


    }
}
