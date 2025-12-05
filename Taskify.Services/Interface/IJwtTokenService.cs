using Taskify.Domain.Entities;

namespace Taskify.Services.Interface
{
    public interface IJwtTokenService
    {
        // Include user's security stamp so tokens can be invalidated when stamp changes
        string GetJwtToken(IEnumerable<string> roles, string userId, string username, string securityStamp);
    }
}
