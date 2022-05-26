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
            ApplicationUser guestUser = await _userManager.FindByNameAsync("guest03@gmail.com");
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
