using Taskify.Domain.Enum;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class UpdateStatusDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public PriorityLevel Priority { get; set; }
        public string? Status { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = default!;
        public string? DueDate { get; set; }
        public string? CreatedBy { get; set; } = default;
        public List<UserSummaryDto> AssignedUser { get; set; } = new List<UserSummaryDto>();
    }
}
