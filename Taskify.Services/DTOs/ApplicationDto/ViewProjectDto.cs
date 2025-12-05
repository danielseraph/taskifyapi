namespace Taskify.Services.DTOs.ApplicationDto
{
    public class ViewProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public DateTime CreateAT { get; set; }
        public int TotalMembers { get; set; }
        public int TotalTasks { get; set; }
        public int TaskInProgress { get; set; }
        public int CompletedTasks { get; set; }
        public int ProgressPercentage { get; set; }
        public List<ProjectMemberDto> Members { get; set; } = new();
        public List<TaskDto> Tasks { get; set; } = new();
    }
}

