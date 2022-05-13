using BugTracker.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Models
{
    public class SeedData
    {
        //make password: Password!1
        public async static Task Initialize(IServiceProvider serviceProvider)
        {
            var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (!context.Roles.Any())
            {
                //create new roles

                List<string> newRoles = new List<string>()
                {
                    "Admin",
                    "Project Manager",
                    "Developer",
                    "Submitter"
                };
                foreach (string role in newRoles)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            if (!context.Users.Any())
            {
                //create new users
                var passwordHasher = new PasswordHasher<ApplicationUser>();

                //user1
                ApplicationUser firstUser = new ApplicationUser()
                {
                    Email = "musab03@gmail.com",
                    NormalizedEmail = "MUSAB03@GMAIL.COM",
                    UserName = "musab03@gmail.com",
                    NormalizedUserName = "MUSAB03@GMAIL.COM",
                    EmailConfirmed = true,
                };
                var firstUserHashedPassword = passwordHasher.HashPassword(firstUser, "Password!1");
                firstUser.PasswordHash = firstUserHashedPassword;
                await userManager.CreateAsync(firstUser);
                //await userManager.AddToRoleAsync(firstUser, "Developer");

                //user2
                ApplicationUser secondUser = new ApplicationUser()
                {
                    Email = "admin03@gmail.com",
                    NormalizedEmail = "ADMIN03@GMAIL.COM",
                    UserName = "admin03@gmail.com",
                    NormalizedUserName = "ADMIN03@GMAIL.COM",
                    EmailConfirmed = true,
                };
                var secondUserHashedPassword = passwordHasher.HashPassword(secondUser, "Password!1");
                secondUser.PasswordHash = secondUserHashedPassword;

                await userManager.CreateAsync(secondUser);
                await userManager.AddToRoleAsync(secondUser, "Admin");
                //await userManager.AddToRoleAsync(secondUser, "Developer");

                //user3: testUser is just a user(potential to become a Developer, ProjectManager, Submitter)
                ApplicationUser testUser = new ApplicationUser()
                {
                    Email = "testuser03@gmail.com",
                    NormalizedEmail = "TESTUSER03@GMAIL.COM",
                    UserName = "testuser03@gmail.com",
                    NormalizedUserName = "TESTUSER03@GMAIL.COM",
                    EmailConfirmed = true,
                };
                var testUserHashedPassword = passwordHasher.HashPassword(testUser, "Password!1");
                testUser.PasswordHash = testUserHashedPassword;
                await userManager.CreateAsync(testUser);
            }
        }
    }
}
