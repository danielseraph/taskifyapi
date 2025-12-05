using Taskify.Domain.Entities;
using Taskify.Domain.Enum;
using TaskStatus = Taskify.Domain.Enum.TaskStatus;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string ProjectName { get; set; } = default!;
        public Guid ProjectId { get; set; }
        public string? CreatedBy { get; set; } = default;
        public List<UserSummaryDto> AssignedUser { get; set; } = new List<UserSummaryDto>();
        public DateTime CreateAt { get; set; }
    }
}
