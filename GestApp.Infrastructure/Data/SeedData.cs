using GestApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GestApp.Infrastructure.Data;

public class SeedData
{
    public static async Task SeedRolesAndUser(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = [
            Roles.Admin.ToString(),
            Roles.Responsable.ToString(),
            Roles.Operator.ToString()
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var email = "admin@gmail.com";
        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            var admin = new ApplicationUser
            {
                FiscalCode = "ADMIN0000000000",
                Name = "Admin",
                Surname = "User",
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                EmailConfirmed = false,
                LockoutEnabled = false
            };

            var result = await userManager.CreateAsync(admin, "Password.1");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, roles.First());
            }
        }
    }
}