using Taskify.Domain.Enum;
using TaskStatus = Taskify.Domain.Enum.TaskStatus;

namespace Taskify.Domain.Entities
{
    public class TaskItem : BaseEntity 
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.ToDo;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public DateTime? DueDate { get; set; } = DateTime.UtcNow;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = default!;

        public string? CreatedByUserId { get; set; }
        public AppUser CreatedByUser { get; set; } = default!;

        public bool IsDeleted { get; set; } = false;

        public ICollection<Comments> Comments { get; set; } = new List<Comments>();
        public ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
    }
}
