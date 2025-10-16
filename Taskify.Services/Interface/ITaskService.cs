using System.Threading.Tasks;
using Taskify.Services.DTOs;
using Taskify.Services.DTOs.ApplicationDto;

namespace Taskify.Services.Interface
{
    public interface ITaskService
    {
        Task<ApiResponse<TaskDto>> CreateTaskAsync(TaskCreateDto model);
        //Task<IEnumerable<TaskDto>> GetAllTask();
        Task<IEnumerable<TaskDto>> GetAllTask(string? filter = null);
        Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(Guid taskId, TaskUpdateDto model);
        Task<ApiResponse<TaskDto>> UpdateTaskAsync(Guid taskId, TaskCreateDto model);
        Task<ApiResponse<TaskDto>> DeleteTaskAsync(Guid Id);
    }
}
