using Taskify.Domain.Entities;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class ProjectUpdateDto : BaseEntity
    {
        public string? ProjectName { get; set; }
        public string? Description { get; set; }
        public int? Task { get; set; }
    }
}
