using AutoMapper;
using Taskify.Domain.Entities;
using Taskify.Services.DTOs;
using Taskify.Services.DTOs.ApplicationDto;

namespace Taskify.Services.MappService
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        { 
            CreateMap<AppUser, RegisterDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Project, ProjectCreateDto>().ReverseMap();
            CreateMap<TaskItem, AssignTaskDto>().ReverseMap();
            CreateMap<TaskCreateDto, TaskItem>().ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId));
            CreateMap<TaskItem, TaskDto>().ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserTasks)); ;
           
        }
    }
}
