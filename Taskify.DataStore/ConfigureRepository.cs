using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taskify.Domain.Entities;
using Taskify.Infrastructure.Persistence;

namespace Taskify.DataStore
{
    public static class ConfigureRepository
    {
        public static IServiceCollection ConfigureRepo(this IServiceCollection service, IConfiguration config)
        {
            service.ConfigDatastore(config);
            service.AddIdentityCore<AppUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
            return service;
        }
    }
}
