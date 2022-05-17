using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        public TicketController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = Db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        //https://localhost:7045/ticket/createTicket
        [HttpGet]
        public IActionResult CreateTicket()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateTicket([Bind("Title, Description, CreatedDate, UpdatedDate, Priority, Status, Type, SubmitterId, ProjectId")] Ticket newTicket)
        {
            //clear validation for properties you need but will not get from the form
            ModelState.ClearValidationState("Project");
            ModelState.ClearValidationState("Submitter");
            Project projectForTicket = _db.Project.Include(project => project.ProjectManager).First(project => project.Id == newTicket.ProjectId);
            ApplicationUser submitter = _db.Users.Include(user => user.ProjectsOwned).First(user => user.Id == newTicket.SubmitterId);

            //add them manually
            newTicket.Project = projectForTicket;
            newTicket.Submitter = submitter;

            //TryValidateModel(newTicket) will only be true once targets on ModelState.ClearValidationState() are either valid or unvalidated.
            //if even one property is invalid the whole model is invalid.
            if (TryValidateModel(newTicket))
            {
                submitter.SubmittedTickets.Add(newTicket);
                projectForTicket.Tickets.Add(newTicket);
                _db.Ticket.Add(newTicket);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
