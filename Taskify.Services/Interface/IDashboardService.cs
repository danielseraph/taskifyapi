using Taskify.Services.DTOs.ApplicationDto;

namespace Taskify.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardData();
    }
}
