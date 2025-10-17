using Taskify.Domain.Entities;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class ProjectDto : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public AppUser? UserName { get; set; }

        public int TotalMembers { get; set; }
        public int TotalTasks { get; set; }
    }
}
