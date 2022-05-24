using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    public class GuestController : Controller
    {
        ApplicationDbContext _db { get; set; }
        UserManager<ApplicationUser> _userManager { get; set; }
        RoleManager<IdentityRole> _roleManager { get; set; }


        public GuestController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
