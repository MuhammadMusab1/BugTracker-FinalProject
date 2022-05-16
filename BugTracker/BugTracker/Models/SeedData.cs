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
                    ProjectsOwned = new HashSet<Project>() //need to be intialized to use the list
                };
                var firstUserHashedPassword = passwordHasher.HashPassword(firstUser, "Password!1");
                firstUser.PasswordHash = firstUserHashedPassword;
                await userManager.CreateAsync(firstUser);
                await userManager.AddToRoleAsync(firstUser, "Project Manager");

                //user2
                ApplicationUser secondUser = new ApplicationUser()
                {
                    Email = "admin03@gmail.com",
                    NormalizedEmail = "ADMIN03@GMAIL.COM",
                    UserName = "admin03@gmail.com",
                    NormalizedUserName = "ADMIN03@GMAIL.COM",
                    EmailConfirmed = true,
                    SubmittedTickets = new HashSet<Ticket>()
                };
                var secondUserHashedPassword = passwordHasher.HashPassword(secondUser, "Password!1");
                secondUser.PasswordHash = secondUserHashedPassword;

                await userManager.CreateAsync(secondUser);
                await userManager.AddToRoleAsync(secondUser, "Admin");
                await userManager.AddToRoleAsync(secondUser, "Submitter");

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
                #region One Project and a Ticket
                if (!context.Project.Any())
                {
                    Project project = new Project()
                    {
                        Name = "SeedData Project",
                        Description = "Project from the SeeData class",
                        ProjectManager = firstUser,
                        ProjectManagerId = firstUser.Id,
                        Tickets = new HashSet<Ticket>(),
                    };
                    firstUser.ProjectsOwned.Add(project); //firstUser is the Project Manager
                    context.Project.Add(project);
                    if (!context.Ticket.Any())
                    {
                        Ticket ticket = new Ticket()
                        {
                            Title = "SeedData Ticket",
                            Description = "This is a ticket from the SeedData class",
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Parse("June 12 2022 12:00"),
                            ProjectId = project.Id,
                            Project = project,
                            Submitter = secondUser,
                            SubmitterId = secondUser.Id,
                        };
                        secondUser.SubmittedTickets.Add(ticket); //secondUser is the submitter
                        project.Tickets.Add(ticket);
                        context.Ticket.Add(ticket);
                    }
                    await userManager.UpdateAsync(firstUser);
                    await userManager.UpdateAsync(secondUser);
                }
                #endregion
            }
        }
    }
}

/*
: Violation of PRIMARY KEY constraint 'PK_AspNetUsers'. 
Cannot insert duplicate key in object 'dbo.AspNetUsers'. 
The duplicate key value is (159e1790-e0ca-40ec-909a-f6ae5e566a5e).

Happend because I was using context.SaveChanges()
So I instead used the userManager to save changes to the context.
 */