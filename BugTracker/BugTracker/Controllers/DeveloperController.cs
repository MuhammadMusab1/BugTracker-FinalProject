using BugTracker.Data;
using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Developer")]
    public class DeveloperController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        private TicketNotificationRepository _ticketNotificationRepo { get; set; }
        private TicketRepository _ticketRepo { get; set; }
        public DeveloperController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = Db;
            _userManager = userManager;
            _roleManager = roleManager;
            _ticketNotificationRepo = new TicketNotificationRepository(Db);
            _ticketRepo = new TicketRepository(Db);

        }
        public async Task<IActionResult> DeveloperDashboard()
        {
            //whenever this method is called: check for notifications of the Developer
            ApplicationUser developer = await _userManager.FindByNameAsync(User.Identity.Name);
            _ticketNotificationRepo.GetList(ticketNot => ticketNot.UserId == developer.Id);
            ViewBag.developer = developer;
            return View(developer.TicketNotifications.ToList());
        }
        public IActionResult TicketNotifications(string developerId)
        {
            List<TicketNotification> ticketNotifications = _ticketNotificationRepo.GetList(ticketNot => ticketNot.UserId == developerId).ToList();
            foreach(TicketNotification ticketNot in ticketNotifications)
            {
                _ticketRepo.Get(ticketNot.TicketId);

            }
            return View(ticketNotifications);
        }
        public IActionResult DeleteNotification(int ticketNotificationId)
        {
            TicketNotification ticketNot = _ticketNotificationRepo.Get(ticketNotificationId);
            _ticketNotificationRepo.Delete(ticketNot);
            _ticketNotificationRepo.Save();
            return RedirectToAction("TicketNotifications", new { developerId = ticketNot.UserId});
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
