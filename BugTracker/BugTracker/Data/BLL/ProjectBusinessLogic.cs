using BugTracker.Data.DAL;
using BugTracker.Models;

namespace BugTracker.Data.BLL
{
    public class ProjectBusinessLogic
    {
        private IRepository<Project> Repo;

        public ProjectBusinessLogic(IRepository<Project> repoArg)
        {
            Repo = repoArg;
        }

        public List<Project> GetAllProjects()
        {
            return Repo.GetAll().ToList();
        }

        public List<Project> GetDevelopersProjects()
        {

        }
    }
}
