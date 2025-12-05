using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskify.Services.DTOs;
using Taskify.Services.DTOs.ApplicationDto;

namespace Taskify.Services.Interface
{
    public interface IProjectService
    {
        Task<ApiResponse<ProjectDto>> CreateProjectAsync(ProjectCreateDto projectDto);
        Task<ApiResponse<ProjectDto>> UpdateProjectAsync(ProjectUpdateDto model);
        Task<ApiResponse<ProjectDto>> DeleteProjectAsync(Guid id);
        Task<ApiResponse<ViewProjectDto>> GetProjectByIdAsync(Guid id);
        Task<IEnumerable<ProjectDto>> GetProjectByNameAsync(string name);
        Task<IEnumerable<ProjectDto>> GetAllProject();
        Task<ApiResponse<ProjectDto>> AddMemberIfNotExistsAsync(Guid projectId, string userId);
        Task<ApiResponse<ViewProjectDto>> OpenProject(Guid projectId);
    }
}
