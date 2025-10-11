using Taskify.Services.DTOs;

namespace Taskify.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto model);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto model);
    }
}
