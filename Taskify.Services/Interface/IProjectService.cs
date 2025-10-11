using Taskify.Services.DTOs;
using Taskify.Services.DTOs.ApplicationDto;

namespace Taskify.Services.Interface
{
    public interface IProjectService
    {
        Task<ApiResponse<ProjectDto>> CreateProjectAsync(ProjectCreateDto projectDto);
        Task<ApiResponse<ProjectDto>> UpdateProjectAsync(ProjectUpdateDto model);
        Task<ApiResponse<ProjectDto>> DeleteProjectAsync(Guid id);
        Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid id);
        Task<IEnumerable<ProjectDto>> GetProjectByNameAsync(string name);
        Task<IEnumerable<ProjectDto>> GetAllProject();
        Task<ApiResponse<ProjectDto>> AddMemberIfNotExistsAsync(Guid projectId, string userId);
    }
}
