using Taskify.Domain.Enum;

namespace Taskify.Services.DTOs.ApplicationDto
{
    public class TaskUpdateDto
    {
        public Domain.Enum.TaskStatus Status { get; set; }
    }
}
