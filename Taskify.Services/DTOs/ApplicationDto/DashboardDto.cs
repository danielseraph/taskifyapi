namespace Taskify.Services.DTOs.ApplicationDto
{
    public class DashboardDto
    {
        public int TotalProjects { get; set; }
        public int ActiveTask { get; set; }
        public int Completed { get; set; }
        public int TeamMembers { get; set; }
        public double ProjectProgress { get; set; }

        public List<ViewProjectDto> RecentProjects { get; set; } = new List<ViewProjectDto>();
        public List<TaskDto> RecentTasks { get; set; } = new List<TaskDto>();
    }
}
