using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authentication;
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
        private RequestDelegate _requestDelegate { get; set; }


        public GuestController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, RequestDelegate requestDelegate)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _requestDelegate = requestDelegate;
        }

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    if (!context.User.Identity.IsAuthenticated)
        //    {
        //        if (string.IsNullOrEmpty(context.User.FindFirstValue(ClaimTypes.Anonymous)))
        //        {
        //            var claim = new Claim(ClaimTypes.Anonymous, System.Guid.NewGuid().ToString());
        //            context.User.AddIdentity(new ClaimsIdentity(new[] { claim }));

        //            string scheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
        //            await context.SignInAsync(scheme, context.User, new AuthenticationProperties { IsPersistent = false });
        //        }
        //    }
        //    await _requestDelegate(context);
        //}

        public IActionResult Index()
        {
            return View();
        }
    }
}
