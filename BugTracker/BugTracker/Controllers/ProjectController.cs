using BugTracker.Data;
using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugTracker.Controllers
{
    public class ProjectController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private IRepository<Project> _projectRepository { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private ProjectBusinessLogic projBl { get; set; }

        public ProjectController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _projectRepository = new ProjectRepository(_db);
            projBl = new ProjectBusinessLogic(new ProjectRepository(_db), userManager);          
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllProjects()
        {
            List<Project> allProjects = _projectRepository.GetAll().ToList();
            foreach(Project project in allProjects)
            {
                await _userManager.FindByIdAsync(project.ProjectManagerId);
            }
            return View(allProjects);
        }

        public async Task<IActionResult> ProjectDetails(int projectId)
        {
            ApplicationUser currentLoggedInUser = await _userManager.FindByNameAsync(User.Identity.Name);
            Project project = _projectRepository.Get(projectId);
            await _userManager.FindByIdAsync(project.ProjectManagerId);
            if(currentLoggedInUser.Id == project.ProjectManagerId)
            {
                ViewBag.CurrentUserIsProjectManager = true;
            }
            else
            {
                ViewBag.CurrentUserIsProjectManager = false;
            }
            if(await _userManager.IsInRoleAsync(currentLoggedInUser, "Admin"))
            {
                ViewBag.CurrentUserIsAdmin = true;
            }
            else
            {
                ViewBag.CurrentUserIsAdmin = false;
            }
            return View(project);
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [Authorize(Roles = "Project Manager")]
        public IActionResult ProjManagerDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Project Manager, Admin")]
        public async Task<IActionResult> ListProjectsPM()
        {
            ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            List<Project> PMprojects = _projectRepository.GetList(p => p.ProjectManagerId == user.Id).ToList();
            return View(PMprojects);
        }

        [Authorize(Roles = "Admin, Project Manager")]
        [HttpGet]
        public IActionResult CreateProject()   
        {
            return View();
        }

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

                    return RedirectToAction("ProjManagerDashboard");
                }
            }
            catch (Exception ex)
            {
                return NotFound("Something went wrong, please try again");
            }
            return View("Index");
        }

        [Authorize(Roles ="Project Manager, Admin")]
        [HttpGet]
        public IActionResult UpdateProject(int projId)
        {
            Project project = projBl.ProjectRepo.Get(projId);
            return View(project);
        }

        [HttpPost]
        public IActionResult UpdateProject(int projId, string? name, string? description)
        {
            Project project = projBl.ProjectRepo.Get(projId);
            try
            {
                project.Name = name;
                project.Description = description;
                projBl.ProjectRepo.Update(project);
                projBl.ProjectRepo.Save();
                return RedirectToAction("ProjManagerDashboard");
            }
            catch (Exception ex)
            {
                return NotFound("Something went wrong, please try again");
            }
        }

        [Authorize(Roles = "Project Manager, Admin")]
        public async Task<IActionResult> AssignDeveloperToProject()
        {
            ApplicationUser currentProjectManager = await _userManager.FindByNameAsync(User.Identity.Name);
            List<ApplicationUser> developers = new List<ApplicationUser>(await _userManager.GetUsersInRoleAsync("Developer"));
            List<Project> projectManagerProjects = _projectRepository.GetList(project => project.ProjectManagerId == currentProjectManager.Id).ToList();
            if (!developers.Any())
            {
                ViewBag.DeveloperMsg = "No Developers in the database!";
            }
            if(!projectManagerProjects.Any())
            {
                ViewBag.ProjectMsg = "No Projects in the database by this project Manager!";
            }
            ViewBag.DeveloperList = new SelectList(developers, "Id", "UserName");
            ViewBag.ProjectList = new SelectList(projectManagerProjects, "Id", "Name");
            return View();
        }
     
        [HttpPost]
        public async Task<IActionResult> AssignDeveloperToProject(int projId, string devId)
        {
            ApplicationUser currentProjectManager = await _userManager.FindByNameAsync(User.Identity.Name);
            List<ApplicationUser> developers = new List<ApplicationUser>(await _userManager.GetUsersInRoleAsync("Developer"));
            List<Project> projectManagerProjects = _projectRepository.GetList(project => project.ProjectManagerId == currentProjectManager.Id).ToList();
            if (!developers.Any())
            {
                ViewBag.DeveloperMsg = "No Developers in the database!";
            }
            if (!projectManagerProjects.Any())
            {
                ViewBag.ProjectMsg = "No Projects in the database by this project Manager!";
            }
            ViewBag.DeveloperList = new SelectList(developers, "Id", "UserName");
            ViewBag.ProjectList = new SelectList(projectManagerProjects, "Id", "Name");
            string message = projBl.AddDeveloperToProject(devId, projId).Result;
            ViewBag.Message = message;
            return View();
        }

        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> DeveloperProject()
        {
            ApplicationUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            try
            {
                Project project = _projectRepository.Get(p => p.Id == user.ProjectAssignedId);
                return View(project);
            }
            catch (Exception)
            {
                return View();
            }          
        }
    }
}
