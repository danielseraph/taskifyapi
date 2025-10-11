using Taskify.Domain.Entities;

namespace Taskify.Services.Interface
{
    public interface IJwtTokenService
    {
        string GetJwtToken(IEnumerable<string> roles, string userId, string username);
    }
}
