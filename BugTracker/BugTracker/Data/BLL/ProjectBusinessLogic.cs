using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Data.BLL
{
    public class ProjectBusinessLogic
    {
        private IRepository<Project> Repo;
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;

        public ProjectBusinessLogic(IRepository<Project> repoArg, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            Repo = repoArg;
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public List<Project> GetAllProjects()
        {
            return Repo.GetAll().ToList();
        }
    }
}
