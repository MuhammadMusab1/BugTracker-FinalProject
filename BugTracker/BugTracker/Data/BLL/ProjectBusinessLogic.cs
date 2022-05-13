using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Data.BLL
{
    public class ProjectBusinessLogic
    {
        private IRepository<Project> ProjectRepo;
        private IRepository<Ticket> TicketRepo;
        private ApplicationDbContext Db;
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;

        public ProjectBusinessLogic(ApplicationDbContext db, IRepository<Project> projRepo, IRepository<Ticekt> ticketRepo, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            Db = db;
            ProjectRepo = projRepo;
            TicketRepo = ticketRepo;
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public List<Project> GetAllProjects()
        {
            return ProjectRepo.GetAll().ToList();
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public async void AddDeveloperToProject(string devId, int projId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(devId);
            Project project = ProjectRepo.Get(projId);
            project.Developers.Add(user);
            user.ProjectAssigned = project;
            ProjectRepo.Save();
            Db.SaveChanges();
        }

        public void AddTicketToProject(int ticketId, int projId)
        {
            Ticket ticket = TicketRepo.Get(ticketId);
            Project project = ProjectRepo.Get(projId);

            project.Tickets.Add(ticket);
            ticket.Project = project;
            ProjectRepo.Save();
            TicketRepo.Save();
        }
    }
}
