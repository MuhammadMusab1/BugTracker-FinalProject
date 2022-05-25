using BugTracker.Data;
using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    public class DeveloperController : Controller
    {
        [Authorize(Roles = "Developer")]
        public IActionResult DeveloperDashboard()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
