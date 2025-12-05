using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taskify.DataStore;
using Taskify.DataStore.Repositorise.Implementation;
using Taskify.DataStore.Repositorise.Interface;
using Taskify.Services.Implementation;
using Taskify.Services.Interface;
using Taskify.Services.MappService;

namespace Taskify.Services
{
    public static class ConfigurationService
    {
        public static IServiceCollection ConfigService(this IServiceCollection services, IConfiguration config)
        {
            services.ConfigureRepo(config);

            //AutoMapper
            
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            //Repositories
            services.AddScoped(typeof(IAppRepository<>), typeof(AppRepository<>));

            //Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddTransient<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUserService,  UserService>();
            services.AddScoped<IAIService, OpenAiService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IFileService, FileService>();
            //HttpContext accessor for getting current user
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserServie>();
            return services;

        }
    }
}
