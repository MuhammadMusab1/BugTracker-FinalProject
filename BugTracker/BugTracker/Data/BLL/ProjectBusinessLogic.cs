using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Data.BLL
{
    public class ProjectBusinessLogic
    {
        public IRepository<Project> ProjectRepo;
        private IRepository<Ticket> TicketRepo;
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;

        public ProjectBusinessLogic(IRepository<Project> repoArg)
        {
            ProjectRepo = repoArg;
        }

        public ProjectBusinessLogic(IRepository<Project> repoArg, UserManager<ApplicationUser> userManager)
        {
            ProjectRepo = repoArg;
            UserManager = userManager;
        }

        public ProjectBusinessLogic(IRepository<Project> projRepo, IRepository<Ticket> ticketRepo, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            ProjectRepo = projRepo;
            TicketRepo = ticketRepo;
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public List<Project> GetAllProjects()
        {
            return ProjectRepo.GetAll().ToList();
        }

        public List<Project> GetAllProjectsFromProjectManager(string pmId)
        {
            return ProjectRepo.GetList(p => p.ProjectManagerId == pmId).ToList();
        }

        public async Task<List<Project>> GetAllProjectsFromDeveloper(string developerId)
        {
            ApplicationUser dev = await UserManager.FindByIdAsync(developerId);
            return ProjectRepo.GetList(p => p.Developers.First(d => d.Id == developerId) == dev).ToList();
        }

        [Authorize(Roles = "Admin, Project Manager")]
        public async void AddDeveloperToProject(string devId, int projId)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(devId);
            Project project = ProjectRepo.Get(projId);
            project.Developers.Add(user);
            user.ProjectAssigned = project; //One to many between Developer and Project(ProjectAssigned is the assigned project to a Developer)
            user.ProjectAssignedId = project.Id;
            ProjectRepo.Save();
        }

        public void AddTicketToProject(int ticketId, int projId)
        {
            Ticket ticket = TicketRepo.Get(ticketId);
            Project project = ProjectRepo.Get(projId);

            project.Tickets.Add(ticket);
            ticket.Project = project;
            ProjectRepo.Save();
        }
    }
}
