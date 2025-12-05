using Taskify.Services.DTOs;

namespace Taskify.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto model, string ipAddress);
        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto model, string ipAddress);
        Task<ApiResponse<string>> LogoutAsync(string userId);
        Task<ApiResponse<string>> AddRoleAsync(string model);

        // Refresh token endpoints
        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<ApiResponse<string>> RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
    }
}
