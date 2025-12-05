namespace Taskify.Domain.Entities
{
    public class UserProject
    {
        public string? UserId { get; set; }
        public AppUser User { get; set; } = default!;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public  DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
