using Microsoft.AspNetCore.Identity;
using RE_Waste.Models;

namespace RE_Waste.Data
{
    // Runs on start-up: makes sure the Manager role and one manager account exist. Safe to run every time.
    public static class IdentitySeeder
    {
        // This method seeds the identity system with a manager role and account. It checks if the role exists, creates it if not, checks for a manager account, creates it if not, and assigns the role to the account.
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            // 1. The role
            if (!await roleManager.RoleExistsAsync(AppRoles.Manager))
            {
                Check(await roleManager.CreateAsync(new IdentityRole(AppRoles.Manager)));
            }

            // 2. The account (skipped if no credentials are configured, e.g. outside Development)
            var email = config["SeedManager:Email"];
            var password = config["SeedManager:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var manager = await userManager.FindByEmailAsync(email);
            if (manager == null)
            {
                manager = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Name = "Re-Waste Manager",
                    EmailConfirmed = true
                };
                Check(await userManager.CreateAsync(manager, password));
            }

            // 3. The link between them
            if (!await userManager.IsInRoleAsync(manager, AppRoles.Manager))
            {
                Check(await userManager.AddToRoleAsync(manager, AppRoles.Manager));
            }
        }

        // Identity reports failures in the result instead of throwing, so turn them into an error we'll notice.
        private static void Check(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Seeding the manager failed: " + string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}