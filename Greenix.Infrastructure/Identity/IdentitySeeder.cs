using Greenix.Domain.Entities;
using Greenix.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace Greenix.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Create Roles
            string[] roles =
            {
                Roles.Administrator,
                Roles.GovernmentOfficer,
                Roles.FieldWorker,
                Roles.Citizen
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create Default Administrator
            var adminEmail = "admin@greenix.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Administrator);
                }
            }
        }
    }
}