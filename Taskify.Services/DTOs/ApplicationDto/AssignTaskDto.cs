using Taskify.Domain.Enum;
using TaskStatus = Taskify.Domain.Enum.TaskStatus;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class AssignTaskDto
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string ProjectName { get; set; } = default!;
        public string? CreatedBy { get; set; } = default;
        public DateTime CreateAt { get; set; }
    }
}
