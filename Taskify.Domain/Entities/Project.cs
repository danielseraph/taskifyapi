namespace Taskify.Domain.Entities
{
    public class Project : BaseEntity
    {
        public string ProjectName { get; set; } = default!;
        public string? Description { get; set; }

        public Guid CreatedByUserId { get; set; } = default!;
        public AppUser CreatedByUser { get; set; } = default!;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<UserProject> UserProjects { get; set; } = new List<UserProject>();
    }
}