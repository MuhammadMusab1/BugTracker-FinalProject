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

        public IActionResult AllProjects()
        {
            return View(_projectRepository.GetAll());
        }

        public IActionResult ProjectDetails(int projectId)
        {
            return View(_projectRepository.Get(projectId));
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Project Manager")]
        public IActionResult ProjManagerDashboard()
        {
            return View(projBl.GetAllProjects());
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

                    return RedirectToAction("ProjManagerDashboard");
                }
            }
            catch (Exception ex)
            {
                return NotFound("Something went wrong, please try again");
            }
            return View("Index");
        }

        [Authorize(Roles ="Project Manager")]
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

        public async Task<IActionResult> AssignDeveloperToProject()
        {
            ViewBag.DeveloperList = new SelectList(await _userManager.GetUsersInRoleAsync("Developer"), "Id", "UserName");
            ViewBag.ProjectList = new SelectList(_projectRepository.GetAll(), "Id", "Name");
            return View();
        }

        [Authorize(Roles = "Project Manager, Admin")]
        [HttpPost]
        public async Task<IActionResult> AssignDeveloperToProject(int projId, string devId)
        {
            ViewBag.DeveloperList = new SelectList(await _userManager.GetUsersInRoleAsync("Developer"), "Id", "UserName");
            ViewBag.ProjectList = new SelectList(_projectRepository.GetAll(), "Id", "Name");
            try
            {
                projBl.AddDeveloperToProject(devId, projId);
                ViewBag.Message = "Successfully assigned developer to project.";
            }
            catch
            {
                ViewBag.Message = "Could not assign developer.";
            }
            return View();
        }
    }
}
