using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace BugTracker.Controllers
{
    public class GuestController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private SignInManager<ApplicationUser> _signInManager { get; set; }


        public GuestController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> LogIn()
        {
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            ApplicationUser firstUser = new ApplicationUser()
            {
                Email = "guest@gmail.com",
                NormalizedEmail = "GUEST@GMAIL.COM",
                UserName = "guest@gmail.com",
                NormalizedUserName = "GUEST@GMAIL.COM",
                EmailConfirmed = true,
                ProjectsOwned = new HashSet<Project>()
            };
            var firstUserHashedPassword = passwordHasher.HashPassword(firstUser, "Password!1");
            firstUser.PasswordHash = firstUserHashedPassword;
            await _userManager.CreateAsync(firstUser);
            await _userManager.AddToRoleAsync(firstUser, "Admin");

            var result = await _signInManager.PasswordSignInAsync("guest@gmail.com", "Password!1", isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Admin/AdminDashboard");
                //NavigationManager.NavigateTo("/Admin/AdminDashboard", true);
            }
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
