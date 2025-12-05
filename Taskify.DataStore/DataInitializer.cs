using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Taskify.DataStore
{
    public class DataInitializer
    {
        public static async Task SeedDatabase(IServiceProvider sp)
        {
            using var scope = sp.CreateScope();
            var scopeService = scope.ServiceProvider;

            var roleManager = scopeService.GetRequiredService<RoleManager<IdentityRole>>();

            //Reading from the json files to seed the roles into the database
            var primaryData = AppDomain.CurrentDomain.BaseDirectory;
            var rolePath = primaryData + "/primaryData/roles.json";

            if(!File.Exists(rolePath))
            {
                throw new FileLoadException($"The {rolePath} does not exist");
            }
            var fileContent = File.ReadAllText(rolePath);
            var roleArr = JsonSerializer.Deserialize<List<string>>(fileContent);
            foreach(var role in roleArr)
            {
                if (roleManager.RoleExistsAsync(role).Result)
                    continue;
                await roleManager.CreateAsync(new IdentityRole
                {
                    Name = role
                });
            }
         
        }
    }
}
