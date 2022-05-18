using BugTracker.Data;
using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    public class ProjectController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private ProjectBusinessLogic projBl { get; set; }

        public ProjectController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;

            projBl = new ProjectBusinessLogic(new ProjectRepository(_db));
            
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin, Project Manager")]
        [HttpGet]
        public IActionResult CreateProject()
        
        {
            return View();
        }
        [Authorize(Roles ="Admin, Project Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateProject(string name, string description)
        {

            try
            {
                ApplicationUser user = await _userManager.GetUserAsync(User);

                if(User != null)
                {
                    var project = new Project
                    {
                        Name = name,
                        Description = description,
                        ProjectManagerId = user.Id,
                        ProjectManager = user
                    };
                    projBl.ProjectRepo.Add(project);
                    projBl.ProjectRepo.Save();

                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
            return View("Index");
        }
    }
}
