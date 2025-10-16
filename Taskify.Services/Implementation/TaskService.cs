using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Taskify.DataStore.Repositorise.Implementation;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.DTOs.ApplicationDto;
using Taskify.Services.Interface;
namespace Taskify.Services.Implementation
{
    public class TaskService : ITaskService
    {
        private readonly IAppRepository<TaskItem> _taskService;
        private readonly IProjectService _projectService;
        private readonly UserManager<AppUser> _appUser;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        public TaskService(IAppRepository<TaskItem> taskService, UserManager<AppUser> appUser, IMapper mapper,ICurrentUserService currentUserService,IProjectService projectService)
        {
            _taskService = taskService;
            _appUser = appUser;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _projectService = projectService;
        }

        public async Task<ApiResponse<TaskDto>> CreateTaskAsync(TaskCreateDto model)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null)
                return ApiResponseBuilder.Fail<TaskDto>("User not authenticated", StatusCodes.Status401Unauthorized);

            var user = await _appUser.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseBuilder.Fail<TaskDto>("User not found", StatusCodes.Status404NotFound);

            var projectResult = await _projectService.GetProjectByIdAsync(model.ProjectId);
            if (!projectResult.IsSuccessful || projectResult.Data == null)
                return ApiResponseBuilder.Fail<TaskDto>("Project not found", StatusCodes.Status404NotFound);

            var project = projectResult.Data;

            // Map DTO → Entity
            var taskEntity = _mapper.Map<TaskItem>(model);
            taskEntity.Id = Guid.NewGuid();
            taskEntity.CreatedByUserId = userId;
            taskEntity.ProjectId = model.ProjectId;
            taskEntity.CreateAT = DateTime.UtcNow;
            taskEntity.UserTasks = new List<UserTask>();

            var assignedUsers = new List<AppUser>();

            if (model.AssignedUserEmail != null && model.AssignedUserEmail.Any())
            {
                foreach (var email in model.AssignedUserEmail)
                {
                    var assignedUser = await _appUser.FindByEmailAsync(email);
                    if (assignedUser == null)
                        return ApiResponseBuilder.Fail<TaskDto>($"User with email {email} not found.");

                    // Add member to project if not already a member
                    await _projectService.AddMemberIfNotExistsAsync(project.Id, assignedUser.Id);

                    // Add the user-task relationship
                    taskEntity.UserTasks.Add(new UserTask
                    {
                        UserId = assignedUser.Id,
                        TaskItemId = taskEntity.Id
                    });

                    assignedUsers.Add(assignedUser);
                }
            }

            await _taskService.AddAsync(taskEntity);
            await _taskService.SaveChangesAsync();

            // Map back to DTO
            var responseDto = _mapper.Map<TaskDto>(taskEntity);
            responseDto.CreatedBy = user.UserName;
            responseDto.AssignedUser = assignedUsers.Select(ut => new UserSummaryDto
            {
                UserName = ut.UserName,
                Email = ut.Email,
            }).ToList()!;

            return ApiResponseBuilder.Success(responseDto, "Task created successfully", StatusCodes.Status201Created);
        }

        public async Task<ApiResponse<TaskDto>> UpdateTaskAsync(Guid taskId, TaskCreateDto model)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null)
                return ApiResponseBuilder.Fail<TaskDto>("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

            var user = await _appUser.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseBuilder.Fail<TaskDto>("User not found", statusCode: StatusCodes.Status404NotFound);

            var projectResult = await _projectService.GetProjectByIdAsync(model.ProjectId);
            if (!projectResult.IsSuccessful || projectResult.Data == null)
                return ApiResponseBuilder.Fail<TaskDto>("Project not found", statusCode: StatusCodes.Status404NotFound);

            var project = projectResult.Data;

            // Load tracked entity so EF Core will detect collection changes
            var taskEntity = await _taskService.GetAllAsync(trackChanges: true)
                .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);

            if (taskEntity == null)
                return ApiResponseBuilder.Fail<TaskDto>("Task not found", statusCode: StatusCodes.Status404NotFound);

            if (taskEntity.CreatedByUserId != userId)
                return ApiResponseBuilder.Fail<TaskDto>("You do not have permission to update this task", statusCode: StatusCodes.Status403Forbidden);

            // Update task properties
            taskEntity.Title = model.Title;
            taskEntity.Description = model.Description;
            taskEntity.Priority = model.Priority;
            taskEntity.Status = model.Status;
            taskEntity.DueDate = model.DueDate;
            taskEntity.UpdateAT = DateTime.UtcNow;
            taskEntity.ProjectId = model.ProjectId;

            // Update assigned users
            var incomingEmails = model.AssignedUserEmail?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
            taskEntity.UserTasks ??= new List<UserTask>();

            var existingEmails = taskEntity.UserTasks
                .Select(ut => ut.User?.Email)
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(e => e!.Trim())
                .ToList();

            // Remove assignments no longer present
            var toRemove = taskEntity.UserTasks
                .Where(ut => ut.User != null && !incomingEmails.Contains(ut.User.Email, StringComparer.OrdinalIgnoreCase))
                .ToList();
            foreach (var remove in toRemove)
            {
                taskEntity.UserTasks.Remove(remove);
            }

            // Add new assignments
            foreach (var email in incomingEmails)
            {
                if (existingEmails.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var assignedUser = await _appUser.FindByEmailAsync(email);
                if (assignedUser == null)
                    return ApiResponseBuilder.Fail<TaskDto>($"User with email {email} not found.");

                var addMemberResult = await _projectService.AddMemberIfNotExistsAsync(project.Id, assignedUser.Id);
                if (!addMemberResult.IsSuccessful)
                    return ApiResponseBuilder.Fail<TaskDto>($"Failed to add user with email {email} to project: {addMemberResult.Message}");

                taskEntity.UserTasks.Add(new UserTask
                {
                    UserId = assignedUser.Id,
                    TaskItemId = taskEntity.Id,
                    User = assignedUser
                });
            }

            // Persist changes
            await _taskService.UpdateAsync(taskEntity);
            var saved = await _taskService.SaveChangesAsync();
            if (!saved)
                return ApiResponseBuilder.Fail<TaskDto>("Failed to update task", statusCode: StatusCodes.Status500InternalServerError);

            // Re-query to ensure navigation props are populated
            var refreshed = await _taskService.GetAllAsync()
                .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskEntity.Id);

            var responseSource = refreshed ?? taskEntity;

            var responseDto = _mapper.Map<TaskDto>(responseSource);
            responseDto.CreatedBy = user.UserName;
            responseDto.AssignedUser = responseSource.UserTasks.Select(ut => new UserSummaryDto
            {
                UserName = ut.User?.UserName,
                Email = ut.User?.Email
            }).ToList()!;

            return ApiResponseBuilder.Success(responseDto, "Task updated successfully");
        }


        public async Task<IEnumerable<TaskDto>> GetAllTask(string? filter = null)
        {
            var userId = _currentUserService.GetUserId();

            var query = _taskService.GetAllAsync(trackChanges: false)
                .Include(t => t.UserTasks)
                .ThenInclude(u => u.User)
                .Include(t => t.Project)
                .ThenInclude(ut => ut.CreatedByUser)
                .AsQueryable()
                .Where(t => !t.IsDeleted);

            if (filter == "created")
                query = query.Where(t => t.CreatedByUserId == userId);
            else if (filter == "assigned")
                query = query.Where(t => t.UserTasks.Any(ut => ut.UserId == userId));
            else
                query = query.Where(t => t.CreatedByUserId == userId ||
                                 t.UserTasks.Any(ut => ut.UserId == userId));

            var task = await query.ToListAsync();

            return task.Select(t => new TaskDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                ProjectName = t.Project.ProjectName,
                CreatedBy = userId,
                AssignedUser = t.UserTasks.Select(ut => new UserSummaryDto
                {
                    UserName = ut.User?.UserName,
                    Email = ut.User?.Email
                }).ToList(),
                DueDate = t.DueDate,
                CreateAt = t.CreateAT
            });
        }
        public async Task<ApiResponse<TaskDto>> UpdateTaskStatusAsync(Guid taskId, TaskUpdateDto model)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null)
                return ApiResponseBuilder.Fail<TaskDto>("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);
            var task = await _taskService.GetAllAsync().Include(t => t.UserTasks).FirstOrDefaultAsync(t => t.Id == taskId && !t.IsDeleted);
            if (task == null)
                return ApiResponseBuilder.Fail<TaskDto>("Task not found", StatusCodes.Status404NotFound);

            //Authorization check — user must be creator or assigned
            var isAuthorized = task.CreatedByUserId == userId || task.UserTasks.Any(ut => ut.UserId == userId);
            if (!isAuthorized)
                return ApiResponseBuilder.Fail<TaskDto>("You are not authorized to update this task", StatusCodes.Status403Forbidden);

            //Update status
            task.Status = (Domain.Enum.TaskStatus)model.Status; 
            task.UpdateAT = DateTime.UtcNow;

            await _taskService.UpdateAsync(task);
            await _taskService.SaveChangesAsync();

            var responseDto = _mapper.Map<TaskDto>(task);
            return ApiResponseBuilder.Success(responseDto, "Task status updated successfully");
        }

        public async Task<ApiResponse<TaskDto>> DeleteTaskAsync(Guid Id)
        {
            var userId = _currentUserService.GetUserId();
            if (userId == null)
                return ApiResponseBuilder.Fail<TaskDto>("User not authenticated", statusCode: StatusCodes.Status401Unauthorized);

            var user = await _appUser.FindByIdAsync(userId);
            if (user == null)
                return ApiResponseBuilder.Fail<TaskDto>("User not found", statusCode: StatusCodes.Status404NotFound);

            var task = await _taskService.GetAllAsync(trackChanges: true)
                .Include(t => t.UserTasks)
                .ThenInclude(ut => ut.User)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == Id && !t.IsDeleted);

            if (task == null)
                return ApiResponseBuilder.Fail<TaskDto>("Task not found", statusCode: StatusCodes.Status404NotFound);

            if (task.CreatedByUserId != userId)
                return ApiResponseBuilder.Fail<TaskDto>("You do not have permission to delete this task", statusCode: StatusCodes.Status403Forbidden);

            await _taskService.DeleteAsync(task);
            var saved = await _taskService.SaveChangesAsync();
            if (!saved)
                return ApiResponseBuilder.Fail<TaskDto>("Failed to delete task", statusCode: StatusCodes.Status500InternalServerError);

            var responseDto = _mapper.Map<TaskDto>(task);
            responseDto.CreatedBy = user.UserName;
            responseDto.AssignedUser = task.UserTasks?.Select(ut => new UserSummaryDto
            {
                UserName = ut.User?.UserName,
                Email = ut.User?.Email
            }).ToList() ?? new List<UserSummaryDto>();

            return ApiResponseBuilder.Success(responseDto, "Task deleted successfully");
        }
    }
}
