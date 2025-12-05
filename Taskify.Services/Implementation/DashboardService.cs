using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.DTOs.ApplicationDto;
using Taskify.Services.Interface;

namespace Taskify.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;

        // Changed parameter type from concrete CurrentUserServie -> ICurrentUserService to match DI registration
        public DashboardService(ITaskService taskService, IProjectService projectService)
        {
            _taskService = taskService;
            _projectService = projectService;
        }

        public async Task<DashboardDto> GetDashboardData()
        {
            // Implementation can call services; keep minimal for now
            var projects = (await _projectService.GetAllProject())?.ToList() ?? new List<ProjectDto>();
            var tasks = (await _taskService.GetAllTask())?.ToList() ?? new List<TaskDto>();

            var completedStatus = Taskify.Domain.Enum.TaskStatus.Done;

            var totalProjects = projects.Count;
            var completed = tasks.Count(t => t.Status == completedStatus);
            var activeTask = tasks.Count(t => t.Status != completedStatus);
            var teamMembers = projects.Sum(p => p.TotalMembers);
            var progress = activeTask == 0 ? 0 : (int)((double)completed / activeTask * 100);

            var tasksByProject = tasks
                .GroupBy(t => t.ProjectId)
                .ToDictionary(g => g.Key, g => new
                {
                    Total = g.Count(),
                    InProgress = g.Count(t => t.Status != completedStatus),
                    Completed = g.Count(t => t.Status == completedStatus)
                });



            var recentProjects = projects
                 .OrderByDescending(p => p.CreateAT)
                 .Take(5)
                 .Select(p =>
                 {
                     // Get task stats for this project
                     tasksByProject.TryGetValue(p.Id, out var stats);

                     int total = stats?.Total ?? 0;
                     int completedCount = stats?.Completed ?? 0;

                     // Calculate project progress (completed tasks out of total tasks)
                     int projectProgress = total == 0
                         ? 0
                         : (int)((double)completedCount / total * 100);

                     return new ViewProjectDto
                     {
                         Name = p.Name,
                         CreateAT = p.CreateAT,
                         TotalMembers = p.TotalMembers,
                         ProgressPercentage = projectProgress,
                         TotalTasks = total,
                         TaskInProgress = stats?.InProgress ?? 0,
                         CompletedTasks = completedCount,
                         Members = new List<ProjectMemberDto>()
                     };
                 })
                 .ToList();


            var recentTasks = tasks
                .OrderByDescending(t => t.CreateAt)
                .Take(10)
                .ToList();



            return new DashboardDto
            {
                TotalProjects = totalProjects,
                ActiveTask = activeTask,
                Completed = completed,
                TeamMembers = teamMembers,
                RecentProjects = recentProjects,
                RecentTasks = recentTasks
            };
        }
    }
}