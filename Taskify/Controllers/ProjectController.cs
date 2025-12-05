using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskify.Domain.Constant;
using Taskify.Services.DTOs.ApplicationDto;
using Taskify.Services.Interface;

[Route("api/projects")]
[ApiController]
[Authorize(Roles = UserRole.PROJECTMANAGER)]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    // POST /api/projects
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto dto)
    {
        var project = await _projectService.CreateProjectAsync(dto);
        return StatusCode(project.StatusCode, project);
    }

    // DELETE /api/projects/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var project = await _projectService.DeleteProjectAsync(id);
        return StatusCode(project.StatusCode, project);
    }

    // PUT /api/projects/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] ProjectUpdateDto model)
    {
        var project = await _projectService.UpdateProjectAsync(model);
        return StatusCode(project.StatusCode, project);
    }

    // GET /api/projects/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        return StatusCode(project.StatusCode, project);
    }

    // GET /api/projects
    [HttpGet]
    public async Task<IActionResult> GetAllProjects()
    {
        var projects = await _projectService.GetAllProject();
        return Ok(projects);
    }

    // GET /api/projects/by-name?name=MyProject
    [HttpGet("by-name")]
    public async Task<IActionResult> GetProjectByName([FromQuery] string name)
    {
        var project = await _projectService.GetProjectByNameAsync(name);
        return Ok(project);
    }

    // GET /api/projects/{id}
    [HttpGet("open-project{id:guid}")]
    public async Task<IActionResult> OpenProject( Guid id)
    {
        var result = await _projectService.OpenProject(id);
        return Ok(result);
    }
}
