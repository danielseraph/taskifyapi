using Microsoft.AspNetCore.Identity;

namespace Taskify.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImageUrl { get; set; }

        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
        public ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
