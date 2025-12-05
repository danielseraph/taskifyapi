using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Taskify.Services.DTOs.ApplicationDto;
using Taskify.Services.Interface;

namespace Taskify.Api.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto model)
        {
            var project = await _taskService.CreateTaskAsync(model);
            return Ok(project);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTask()
        {
            var task = await _taskService.GetAllTask();
            return Ok(task);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskUpdateDto model)
        {
            var response = await _taskService.UpdateTaskStatusAsync(id, model);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTaskDetails(Guid id, [FromBody] TaskCreateDto model)
        {
            var response = await _taskService.UpdateTaskAsync(id, model);
            return StatusCode(response.StatusCode, response);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var response = await _taskService.DeleteTaskAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> ViewTask(Guid id)
        {
            var response = await _taskService.ViewTask(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
