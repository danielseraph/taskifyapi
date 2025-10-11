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

        //public async Task<ApiResponse<TaskDto>> CreateTaskAsync(TaskCreateDto model)
        //{
        //    var userId = _currentUserService.GetUserId();
        //    if (userId == null)
        //        return ApiResponseBuilder.Fail<TaskDto>(
        //            "User not authenticated",
        //            statusCode: StatusCodes.Status401Unauthorized
        //        );

        //    var user = await _appUser.FindByIdAsync(userId);
        //    if (user == null)
        //        return ApiResponseBuilder.Fail<TaskDto>(
        //            "User not found",
        //            statusCode: StatusCodes.Status404NotFound
        //        );

        //    var project = await _projectService.GetProjectByIdAsync(model.ProjectId);
        //    if (project.IsSuccessful == false)
        //        return ApiResponseBuilder.Fail<TaskDto>(
        //            "Project not found",
        //            statusCode: StatusCodes.Status404NotFound
        //        );

        //    // Map DTO → Entity
        //    var taskEntity = _mapper.Map<TaskItem>(model);
        //    taskEntity.Id = Guid.NewGuid();
        //    taskEntity.CreatedByUserId = userId;
        //    taskEntity.CreateAT = DateTime.UtcNow;


        //    // Add assigned users
        //    var assignedUsers = new List<AppUser>();
        //    //var user = _appUser.FindByIdAsync(model.AssignedUserIds)

        //    if (model.AssignedUserEmail.Any())
        //    {
        //        foreach (var email in model.AssignedUserEmail)
        //        {
        //            var userEmail = await _appUser.FindByEmailAsync(email);
        //            if (userEmail == null)
        //                return ApiResponseBuilder.Fail<TaskDto>($"User with email {email} not found.");
        //            await _projectService.AddMemberIfNotExistsAsync(project., userEmail.Id);
        //            taskEntity.UserTasks.Add(new UserTask
        //            {
        //                UserId = assignedUserId,
        //                TaskItemId = taskEntity.Id
        //            });
        //            var assignedUser = await _appUser.FindByIdAsync(assignedUserId);
        //            var userEmail = await _appUser.FindByEmailAsync(assignedUser.Email);
        //            if (assignedUser != null)
        //            {
        //                assignedUsers.Add(assignedUser);
        //            }
        //        }
        //    }
        //var assignedUser = await _appUser.FindByIdAsync(model.AssignedUserIds);

        //    await _taskService.AddAsync(taskEntity);
        //    await _taskService.SaveChangesAsync();
        //    // Map back to DTO
        //    var responseDto = _mapper.Map<TaskDto>(taskEntity);
        //    responseDto.CreatedBy = user.UserName;
        //    responseDto.AssignedUser = assignedUsers.Select(u => u.UserName).ToList()!;


        //    return ApiResponseBuilder.Success(
        //        responseDto,
        //        "Task created successfully",
        //        statusCode: StatusCodes.Status201Created
        //    );
        //}

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
            responseDto.AssignedUser = assignedUsers.Select(u => u.UserName).ToList()!;

            return ApiResponseBuilder.Success(responseDto, "Task created successfully", StatusCodes.Status201Created);
        }

        public async Task<IEnumerable<TaskDto>> GetAllTask(string? filter = null)
        {
            var userId = _currentUserService.GetUserId();
            var querry = _taskService.GetAllAsync(trackChanges: false)
                .Include(t => t.UserTasks)
                .Include(t => t.Project)
                .ThenInclude(ut => ut.CreatedByUser)
                .AsQueryable();

            if (filter == "created")
                querry = querry.Where(t => t.CreatedByUserId == userId);
            else if (filter == "assigned")
                querry = querry.Where(t => t.UserTasks.Any(ut => ut.UserId == userId));
            else
                querry = querry.Where(t => t.CreatedByUserId == userId ||
                t.UserTasks.Any(ut => ut.UserId == userId));

            var task = await querry.ToListAsync();

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

            // ✅ Authorization check — user must be creator or assigned
            var isAuthorized = task.CreatedByUserId == userId || task.UserTasks.Any(ut => ut.UserId == userId);
            if (!isAuthorized)
                return ApiResponseBuilder.Fail<TaskDto>("You are not authorized to update this task", StatusCodes.Status403Forbidden);

            // ✅ Update status
            task.Status = (Domain.Enum.TaskStatus)model.Status;
            task.UpdateAT = DateTime.UtcNow;

            await _taskService.UpdateAsync(task);
            await _taskService.SaveChangesAsync();

            var responseDto = _mapper.Map<TaskDto>(task);
            return ApiResponseBuilder.Success(responseDto, "Task status updated successfully");
        }
    }
}
