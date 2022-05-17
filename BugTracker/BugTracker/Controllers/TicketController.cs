using BugTracker.Data;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private ProjectRepository _projectRepo { get; set; }
        private TicketRepository _ticketRepo { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        public TicketController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _projectRepo = new ProjectRepository(Db);
            _ticketRepo = new TicketRepository(Db);
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
        public async Task<IActionResult> CreateTicket([Bind("Title, Description, CreatedDate, UpdatedDate, Priority, Status, Type, SubmitterId, ProjectId")] Ticket newTicket)
        {
            //clear validation for properties you need but will not get from the form
            ModelState.ClearValidationState("Project");
            ModelState.ClearValidationState("Submitter");

            Project project = _projectRepo.Get(project => project.Id == newTicket.ProjectId);

            ApplicationUser submitter = await _userManager.FindByIdAsync(newTicket.SubmitterId);
            List<Project> projectsOwnedBySubmitter = _projectRepo.GetList(project => project.ProjectManagerId == submitter.Id).ToList();
            submitter.ProjectsOwned = projectsOwnedBySubmitter;

            ApplicationUser projectManager = await _userManager.FindByIdAsync(project.ProjectManagerId);

            //add them manually to validate the model
            newTicket.Project = project;
            newTicket.Submitter = submitter;

            //TryValidateModel(newTicket) will only be true once targets on ModelState.ClearValidationState() are valid
            //if even one property is invalid the whole model is invalid.
            if (TryValidateModel(newTicket))
            {
                submitter.SubmittedTickets.Add(newTicket);
                project.Tickets.Add(newTicket);
                _ticketRepo.Add(newTicket);
                await _userManager.UpdateAsync(submitter); //since we used _userManager in this method we can't use _ticketRepo.Save()
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
