using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BugTracker.Controllers
{
    public class GuestController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }


        public GuestController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            //user1
            ApplicationUser guestUser = new ApplicationUser()
            {
                Email = "guest03@gmail.com",
                NormalizedEmail = "GUEST03@GMAIL.COM",
                UserName = "guest03@gmail.com",
                NormalizedUserName = "GUEST03@GMAIL.COM",
                EmailConfirmed = true,
                ProjectsOwned = new HashSet<Project>() //need to be intialized to use the list
            };
            var firstUserHashedPassword = passwordHasher.HashPassword(guestUser, "Password!1");
            guestUser.PasswordHash = firstUserHashedPassword;
            await _userManager.CreateAsync(guestUser);
            await _userManager.AddToRoleAsync(guestUser, "Admin");
            return View(guestUser);
        }
        [HttpPost]
        public IActionResult Index(bool re)
        {
            if(re)
            {
                return View("AdminDashboard");
            }
            else
            {
                return View();
            }
        }
    }
}
