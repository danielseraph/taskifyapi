using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Domain.Entities;
using Taskify.Infrastructure.Persistence;
using Taskify.Services.DTOs;
using Taskify.Services.DTOs.ApplicationDto;
using Taskify.Services.Interface;

namespace Taskify.Services.Implementation
{
    public class ProjectService : IProjectService
    {
        private readonly IAppRepository<Project> _appRepository;
        private readonly UserManager<AppUser> _appUser;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        public ProjectService(IAppRepository<Project> appRepository, IMapper mapper, ICurrentUserService currentUserService,UserManager<AppUser> appUser)
        {
            _appRepository = appRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _appUser = appUser;
        }

        public async Task<ApiResponse<ProjectDto>> CreateProjectAsync(ProjectCreateDto projectDto)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null)
            {
                return ApiResponseBuilder.Fail<ProjectDto>(
                    "User not authenticated",
                    statusCode: StatusCodes.Status401Unauthorized
                );
            }

            var user = await _appUser.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseBuilder.Fail<ProjectDto>(
                    "User not found",
                    statusCode: StatusCodes.Status404NotFound
                );
            }

            var existingProject = await _appRepository
                    .FindByCondition(p => p.ProjectName == projectDto.ProjectName
                     && p.CreatedByUserId == Guid.Parse(userId))
                    .FirstOrDefaultAsync();

            if (existingProject != null)
            {
                return ApiResponseBuilder.Fail<ProjectDto>(
                    "You already have a project with this name",
                    statusCode: StatusCodes.Status409Conflict
                );
            }


            var project = _mapper.Map<Project>(projectDto);
            project.CreatedByUserId = Guid.Parse(userId); 
            project.UserProjects.Add(new UserProject
            {
                UserId = userId,
                Project = project
            });

            await _appRepository.AddAsync(project);
            await _appRepository.SaveChangesAsync();

            var response = new ProjectDto
            {
                Id = project.Id,
                Name = project.ProjectName,
                Description = project.Description,
                CreatedBy = project.CreatedByUserId,
                CreateAT = project.CreateAT,
                UpdateAT = project.UpdateAT
            };

            return ApiResponseBuilder.Success(response, "Project created successfully");
        }

        //public async Task<ApiResponse<ProjectDto>> AddMemberIfNotExistsAsync(Guid projectId,  string userId)
        //{
        //    var project = _appRepository.GetAllAsync()
        //        .Include(p => p.UserProjects)
        //        .FirstOrDefaultAsync(u => u.Id == projectId);

        //    if (project == null)
        //        return ApiResponseBuilder.Fail<ProjectDto>("Project not found");
        //    //check if the user is already a member to the project
        //    //bool alreadyMember = await project.
        //    var newMember = new UserProject
        //    {
        //        ProjectId = projectId,
        //        UserId = userId,
        //        JoinedAt = DateTime.UtcNow,

        //    };
        //    await project
        //}
        public async Task<ApiResponse<ProjectDto>> DeleteProjectAsync(Guid id)
        {
            var project = await _appRepository.GetByIdAsync(id);
            if (project == null)
                return ApiResponseBuilder.Fail<ProjectDto>("Project not found", statusCode:StatusCodes.Status404NotFound);
            await _appRepository.DeleteAsync(project);
            await _appRepository.SaveChangesAsync();

            var response = new ProjectDto
            {
                Id = project.Id,
                Name = project.ProjectName,
                Description = project.Description,
                CreateAT = project.CreateAT,
                UpdateAT = project.UpdateAT
            };
            
            return ApiResponseBuilder.Success(
                response,
                "Project deleted successfully"
            );
        }
        public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(Guid id)
        {
            var project = await _appRepository.GetByIdAsync(id);
            if(project == null)
                return ApiResponseBuilder.Fail<ProjectDto>("Project not found",statusCode:StatusCodes.Status404NotFound);
            var result = _mapper.Map<ProjectDto>(project);
            return ApiResponseBuilder.Success(result,"Project retrieved successfully", statusCode:StatusCodes.Status200OK);
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectByNameAsync(string name)
        {
            var projects = await _appRepository.FindByCondition(n => n.ProjectName.ToLower().Contains(name.ToLower())).ToListAsync();
            var projectDtos = projects.Select(project => new ProjectDto
            {
                Id = project.Id,
                Name = project.ProjectName,
                Description = project.Description,
                CreateAT = project.CreateAT,
                UpdateAT = project.UpdateAT
            });
            return projectDtos;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProject()
        {
            var userId = _currentUserService.GetUserId();
            var userGuid = Guid.Parse(userId!);

            var query = _appRepository.GetAllAsync(trackChanges: false)
                                      .Where(p => p.CreatedByUserId == userGuid);
                                     

            var projects = await query.ToListAsync();

            return projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.ProjectName,
                Description = p.Description,
                CreatedBy = p.CreatedByUserId,
                CreateAT = p.CreateAT,
                UpdateAT = p.UpdateAT
            });
        }

        public async Task<ApiResponse<ProjectDto>> UpdateProjectAsync(ProjectUpdateDto model)
        {
            var project = await _appRepository.FindByCondition(p => p.Id == model.Id).FirstOrDefaultAsync();
            if (project == null)
                return ApiResponseBuilder.Fail<ProjectDto>("Project not found");
            project.ProjectName = model.ProjectName!;
            project.Description = model.Description;
            project.CreateAT = model.CreateAT;
            project.UpdateAT = model.UpdateAT;   
            await _appRepository.UpdateAsync(project);
            var isSaved = await _appRepository.SaveChangesAsync();
            if (!isSaved)
                return ApiResponseBuilder.Fail<ProjectDto>("Failed to update", statusCode: StatusCodes.Status400BadRequest);
            var result = _mapper.Map<ProjectDto>(project);
            return ApiResponseBuilder.Success<ProjectDto>(result, "Project Updeated Successfully", statusCode: StatusCodes.Status200OK);
        }

        //public async Task<ApiResponse<ProjectDto>> Delete(Guid id)
        //{
        //    var project = await _appRepository.GetByIdAsync(id);
        //    if (project == null)
        //        return ApiResponseBuilder.Fail<ProjectDto>("Project not found", statusCode: StatusCodes.Status404NotFound);
        //    await _appRepository.DeleteAsync(project);
        //    await _appRepository.SaveChangesAsync();

        //    var response = _mapper.Map<ProjectDto>(project);
        //    return ApiResponseBuilder.Success(response, "Project deleted successfully");
        //}

        public async Task<ApiResponse<ProjectDto>> AddMemberIfNotExistsAsync(Guid projectId, string userId)
        {
            // Load project as tracked so EF will detect collection changes
            var project = await _appRepository.GetAllAsync(trackChanges: true)
                .Include(p => p.UserProjects)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return ApiResponseBuilder.Fail<ProjectDto>("Project not found", statusCode: StatusCodes.Status404NotFound);

            // Check if user exists
            var user = await _appUser.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseBuilder.Fail<ProjectDto>("User not found", statusCode: StatusCodes.Status404NotFound);

            project.UserProjects ??= new List<UserProject>();

            // Check if the user is already a member of this project
            if (project.UserProjects.Any(pm => pm.UserId == userId))
            {
                return ApiResponseBuilder.Success<ProjectDto>(
                    _mapper.Map<ProjectDto>(project),
                    "User already a member of this project"
                );
            }

            // Add the user to the project
            var newMember = new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                User = user
            };

            project.UserProjects.Add(newMember);

            // Mark for update (safe even when tracked) and persist
            await _appRepository.UpdateAsync(project);
            var saved = await _appRepository.SaveChangesAsync();
            if (!saved)
                return ApiResponseBuilder.Fail<ProjectDto>("Failed to add user to project", statusCode: StatusCodes.Status500InternalServerError);

            var result = _mapper.Map<ProjectDto>(project);
            return ApiResponseBuilder.Success(result, "User added to project successfully");
        }

    }
}
