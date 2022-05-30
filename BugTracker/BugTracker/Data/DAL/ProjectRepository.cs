using BugTracker.Models;

namespace BugTracker.Data.DAL
{
    public class ProjectRepository : IRepository<Project>
    {
        private readonly ApplicationDbContext Db;

        public ProjectRepository(ApplicationDbContext db)
        {
            Db = db;
        }

        public void Add(Project project)
        {
            Db.Project.Add(project);
        }

        public void Delete(Project project)
        {
            Db.Project.Remove(project);
        }

        public void Update(Project project)
        {
            Db.Project.Update(project);
        }

        public Project Get(int id)
        {
            return Db.Project.First(p => p.Id == id);
        }

        public Project Get(Func<Project, bool> firstFunction)
        {
            return Db.Project.First(firstFunction);
        }

        public ICollection<Project> GetAll()
        {
            return Db.Project.ToList();
        }

        public ICollection<Project> GetList(Func<Project, bool> whereFunction)
        {
            return Db.Project.Where(whereFunction).ToList();
        }

        public void Save()
        {
            Db.SaveChanges();
        }
    }
}
