using FishingBuddy.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace FishingBuddy.Data;

public static class IdentitySeeder
{
    public static async Task SeedRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var roles = new[] { "Admin", "Manager" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var bootstrapAdminEmails = configuration
            .GetSection("Authorization:BootstrapAdminEmails")
            .Get<string[]>() ?? Array.Empty<string>();

        foreach (var email in bootstrapAdminEmails)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                continue;
            }

            var user = await userManager.FindByEmailAsync(email.Trim());
            if (user == null)
            {
                continue;
            }

            if (!await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }

            if (!await userManager.IsInRoleAsync(user, "Manager"))
            {
                await userManager.AddToRoleAsync(user, "Manager");
            }
        }
    }
}
