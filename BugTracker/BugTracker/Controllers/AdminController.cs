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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private IRepository<Project> _projectRepository { get; set; }
        private ProjectBusinessLogic projBl { get; set; }
        public AdminController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = Db;
            _userManager = userManager;
            _roleManager = roleManager;
            _projectRepository = new ProjectRepository(_db);
            projBl = new ProjectBusinessLogic(new ProjectRepository(_db), userManager);
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AssignRoleToUser()
        {
            ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
            ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignRoleToUser(string? userId, string? role)
        {
            if(userId != null && role != null)
            {
                try
                {
                    string message = "";
                    ApplicationUser user = await _userManager.FindByIdAsync(userId);
                    if (await _roleManager.RoleExistsAsync(role)) // check if role is in the database
                    {
                        if (!await _userManager.IsInRoleAsync(user, role)) //check if the user is already in that role
                        {
                            await _userManager.AddToRoleAsync(user, role);
                            message = $"{user.UserName} has been assigned to {role} role";
                            ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
                            ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
                            return View("AssignRoleToUser", message);
                        }
                        else
                        {
                            message = $"{user.UserName} is already in {role} role";
                            ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
                            ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
                            return View("AssignRoleToUser", message);
                        }
                    }
                    else
                    {
                        message = $"role doesn't exist";
                        ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
                        ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
                        return View("AssignRoleToUser", message);
                    }
                }
                catch(Exception ex)
                {
                    return NotFound("user not Found at AssignRoleToUser post method");
                }
            }
            else
            {
                return BadRequest("role or userId is null at AssignRoleToUser post method");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult UnassignUserFromRole()
        {
            ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
            ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UnassignUserFromRole(string userId, string role)
        {
            if (userId != null && role != null)
            {
                try
                {
                    string message = "";
                    ApplicationUser user = await _userManager.FindByIdAsync(userId);
                    if (await _roleManager.RoleExistsAsync(role)) // check if role is in the database
                    {
                        if (await _userManager.IsInRoleAsync(user, role)) //check if the user is in that role (To remove user)
                        {
                            await _userManager.RemoveFromRoleAsync(user, role);
                            message = $"{user.UserName} has been removed from the {role} role";
                            ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
                            ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
                            return View("UnassignUserFromRole", message);
                        }
                        else
                        {
                            message = $"{user.UserName} is not in the {role} role";
                            ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
                            ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
                            return View("UnassignUserFromRole", message);
                        }
                    }
                    else
                    {
                        message = $"role doesn't exist";
                        ViewBag.usersList = new SelectList(_db.Users.ToList(), "Id", "UserName");
                        ViewBag.rolesList = new SelectList(_db.Roles.ToList(), "Name", "Name");
                        return View("UnassignUserFromRole", message);
                    }
                }
                catch (Exception ex)
                {
                    return NotFound("user not Found at AssignRoleToUser post method");
                }
            }
            else
            {
                return BadRequest("role or userId is null at AssignRoleToUser post method");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignProjectToPM()
        {
            ViewBag.Projects = new SelectList(_projectRepository.GetAll(), "Id", "Name");
            ViewBag.PMs = new SelectList(await _userManager.GetUsersInRoleAsync("Project Manager"), "Id", "UserName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignProjectToPM(int projId, string pmId)
        {
            ViewBag.Projects = new SelectList(_projectRepository.GetAll(), "Id", "Name");
            ViewBag.PMs = new SelectList(await _userManager.GetUsersInRoleAsync("Project Manager"), "Id", "UserName");

            try
            {
                projBl.AssignProjToPM(projId, pmId);
                ViewBag.Message = "Successfully added Project Manager to Project";
            }
            catch
            {
                ViewBag.Message = "Could not assign Project Manager to Project";
            }         
            return View();
        }
    }
}
