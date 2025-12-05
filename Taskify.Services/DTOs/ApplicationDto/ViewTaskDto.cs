using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskify.Domain.Enum;
using TaskStatus = Taskify.Domain.Enum.TaskStatus;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class ViewTaskDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public PriorityLevel Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public string ProjectName { get; set; } = default!;
        public Guid ProjectId { get; set; }

        // legacy simple created by name (kept for backward compatibility)
        public string? CreatedBy { get; set; } = default;

        // New richer objects for the detailed view
        public UserSummaryDto? CreatedByUser { get; set; }
        public List<UserSummaryDto> AssignedUsers { get; set; } = new List<UserSummaryDto>();
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
        public List<object> Attachments { get; set; } = new List<object>();

        // keep existing property used elsewhere
        public List<UserSummaryDto> AssignedUser { get; set; } = new List<UserSummaryDto>();

        public DateTime CreateAt { get; set; }

        // optional updated timestamp
        public DateTime? UpdatedAt { get; set; }
    }
}
