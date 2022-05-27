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

        [Authorize(Roles = "Admin, Project Manager")]
        public async Task<string> AddDeveloperToProject(string devId, int projId)
        {
            try
            {
                ApplicationUser user = await UserManager.FindByIdAsync(devId);
                Project project = ProjectRepo.Get(projId);
                project.Developers.Add(user);
                user.ProjectAssigned = project; //One to many between Developer and Project(ProjectAssigned is the assigned project to a Developer)
                user.ProjectAssignedId = project.Id;
                ProjectRepo.Save();
                return "Successfully assigned developer to project.";
            }
            catch (Exception ex)
            {
                return "Could not assign developer.";
            }
        }

        public async void AssignProjToPM(int projId, string pmId)
        {
            Project project = ProjectRepo.Get(projId);
            ApplicationUser user = await UserManager.FindByIdAsync(pmId);
            project.ProjectManager = await UserManager.FindByIdAsync(pmId);
            project.ProjectManagerId = pmId;
            user.ProjectsOwned = ProjectRepo.GetList(p => p.ProjectManagerId == pmId);
            user.ProjectsOwned.Add(project);
            ProjectRepo.Save();
        }
    }
}
