using Taskify.Domain.Entities;
using Taskify.Services.DTOs;

namespace Taskify.Services.Interface
{
    public interface ICurrentUserService
    {
        string? GetUserId();
        string? GetUserName();
        bool IsAuthenticated();
        Task<CurrentUserDto?> GetCurrentUserAsync();
    }
}
