using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LlmBackend.Auth
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());
            context.Database.EnsureCreated();
            //context.Database.Migrate();
            if (context.Users.Any())
            {
                return;
            }

            string[] roles = ["Administrator", "Manager"];
            using var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            using var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            var user = new AppUser
            {
                Email = "test@test.com",
                NormalizedEmail = "TEST@TEST.COM",
                UserName = "test@test.com",
                NormalizedUserName = "TEST@TEST.COM",
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            await userManager.CreateAsync(user, "Passw0rd!");
            await userManager.AddToRolesAsync(user, roles);

            await context.SaveChangesAsync();
        }
    }
}
