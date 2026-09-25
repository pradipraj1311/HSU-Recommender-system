using Microsoft.AspNetCore.Identity;

namespace HSU_Recommender_system.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            string[] roleNames = { "Admin", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))

                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
            if (adminUsers.Count == 0)
            {
                var adminEmail = config["AdminSeed:Email"];
                var adminPassword = config["AdminSeed:Password"];

                if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
                {
                    var masterAdmin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(masterAdmin, adminPassword);
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(masterAdmin, "Admin");
                    }
                }
            }
        }
    }
}