using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Db;

public static class SeedManager
{
    public static async Task Seed(IServiceProvider services)
    {
        await SeedRoles(services);

        await SeedAdminUser(services);
    }

    private static async Task SeedRoles(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (!roleManager.Roles.Any(r => r.Name == Role.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Role.Admin));
        }

        if (!roleManager.Roles.Any(r => r.Name == Role.User))
        {
            await roleManager.CreateAsync(new IdentityRole(Role.User));
        }
    }
    
    private static async Task SeedAdminUser(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        var adminUser = await context.Users.FirstOrDefaultAsync(user => user.UserName == "AuthenticationAdmin");

        if (adminUser is null)
        {
            adminUser = new User 
                { 
                    UserName = "AuthenticationAdmin",
                    Email = "your@email.com",
                    Provider = Consts.LoginProviders.Password
                };
                await userManager.CreateAsync(adminUser, "VerySecretPassword!1");
            await userManager.AddToRoleAsync(adminUser, Role.Admin);
        }
    }
}