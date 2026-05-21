using AcademicCompliance.Domain.Common;
using AcademicCompliance.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicCompliance.Infrastructure.Data;

public static class IdentityDataSeeder
{
    private const string AdminEmail = "admin@acadeguard.com";
    private const string AdminPassword = "Admin123!";
    private const string AdminFullName = "AcadeGuard Administrator";

    public static async Task SeedIdentityAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var roleName in ApplicationRoles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            EnsureIdentitySucceeded(roleResult);
        }

        var adminUser = await userManager.FindByEmailAsync(AdminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = AdminFullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
            EnsureIdentitySucceeded(createResult);
        }
        else
        {
            var updated = false;

            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                updated = true;
            }

            if (string.IsNullOrWhiteSpace(adminUser.FullName))
            {
                adminUser.FullName = AdminFullName;
                updated = true;
            }

            if (updated)
            {
                var updateResult = await userManager.UpdateAsync(adminUser);
                EnsureIdentitySucceeded(updateResult);
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, ApplicationRoles.Admin))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, ApplicationRoles.Admin);
            EnsureIdentitySucceeded(roleResult);
        }
    }

    private static void EnsureIdentitySucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException(errors);
    }
}
