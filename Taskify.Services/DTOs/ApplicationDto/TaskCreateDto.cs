using Taskify.Domain.Enum;
using TaskStatus = Taskify.Domain.Enum.TaskStatus;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class TaskCreateDto
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; } = string.Empty;
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
        public DateTime? DueDate { get; set; } = DateTime.UtcNow;
        public Guid ProjectId { get; set; }
        public List<string> AssignedUserEmail { get; set; } = new(); 
    }
}
