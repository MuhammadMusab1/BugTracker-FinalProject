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
        private ApplicationDbContext _db { get; set; }
        private ProjectRepository _projectRepo { get; set; }
        private TicketRepository _ticketRepo { get; set; }
        private TicketHistoryRepository _ticketHistoryRepo { get; set; }
        private TicketLogItemRepository _ticketLogItemRepo { get; set; }
        private UserManager<ApplicationUser> _userManager { get; set; }
        private RoleManager<IdentityRole> _roleManager { get; set; }
        public TicketController(ApplicationDbContext Db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = Db;
            _userManager = userManager;
            _roleManager = roleManager;
            _projectRepo = new ProjectRepository(Db);
            _ticketRepo = new TicketRepository(Db);
            _ticketHistoryRepo = new TicketHistoryRepository(Db);
            _ticketLogItemRepo = new TicketLogItemRepository(Db);
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
        //https://localhost:7045/ticket/updateTicket?ticketId=5
        [HttpGet]
        public IActionResult UpdateTicket(int? ticketId)
        {
            if(ticketId != null)
            {
                try
                {
                    Ticket ticket = _ticketRepo.Get(ticket => ticket.Id == ticketId);
                    return View(ticket);
                }
                catch (Exception ex)
                {
                    return NotFound("Ticket cannot be found at UpdateTicket get method");
                }
            }
            else
            {
                return BadRequest("ticketId is null at UpdateTicket get method");
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateTicket(int? ticketId, Ticket updatedTicket)
        {
            if(ticketId != null)
            {
                try
                {
                    ApplicationUser userUpdatingTheTicket = await _userManager.FindByEmailAsync(User.Identity.Name);
                    Ticket ticket = _ticketRepo.Get(ticket => ticket.Id == ticketId);
                    TicketHistory ticketHistory = new TicketHistory();
                    TicketLogItem ticketLogItem = new TicketLogItem();//mirage of the old ticket
                    //Title, Description, Type, status, Priority are the only properties that can be change
                    //Assigning and Unassigning developer to ticket will be handled in a different method
                    if(ticket.Title != updatedTicket.Title)
                    {
                        ticketHistory.PropertiesChanged.Add($"Title({updatedTicket.Title})"); //oldValue(newValue)
                    }
                    if(ticket.Description != updatedTicket.Description)
                    {
                        ticketHistory.PropertiesChanged.Add($"Description({updatedTicket.Description})");
                    }
                    if (ticket.Status != updatedTicket.Status)
                    {
                        ticketHistory.PropertiesChanged.Add($"Status({updatedTicket.Status})");
                    }
                    if (ticket.Priority != updatedTicket.Priority)
                    {
                        ticketHistory.PropertiesChanged.Add($"Priority({updatedTicket.Priority})");
                    }
                    if (ticket.Type != updatedTicket.Type)
                    {
                        ticketHistory.PropertiesChanged.Add($"Type({updatedTicket.Type})");
                    }
                    if(!ticketHistory.PropertiesChanged.Any()) //no properties changed
                    {
                        //don't save ticketHistory, ticketLogItem and update the ticket
                        return View("Index");
                    }
                    else
                    {

                        //make mirage of the oldTicket
                        ticketLogItem.OldTitle = ticket.Title;
                        ticketLogItem.OldDescription = ticket.Description;
                        ticketLogItem.OldStatus = ticket.Status.ToString();
                        ticketLogItem.OldPriority = ticket.Priority.ToString();
                        ticketLogItem.OldType = ticket.Type.ToString();

                        //ticket is now updatedTicket
                        //ticket = updatedTicket; //will a new ticket when saving
                        ticket.Title = updatedTicket.Title;
                        ticket.Description = updatedTicket.Description;
                        ticket.Status = updatedTicket.Status;
                        ticket.Priority = updatedTicket.Priority;
                        ticket.Type = updatedTicket.Type;

                        List<TicketHistory> ticketHistoriesOfTicket = _ticketHistoryRepo.GetList(ticketH => ticketH.TicketId == ticketId).ToList();
                        List<TicketHistory> ticketHistoriesOfUser = _ticketHistoryRepo.GetList(ticketH => ticketH.UserId == userUpdatingTheTicket.Id).ToList();
                        ticket.TicketHistories = ticketHistoriesOfTicket;
                        userUpdatingTheTicket.TicketHistories = ticketHistoriesOfUser;

                        //One to One
                        ticketHistory.TicketLogItem = ticketLogItem;
                        ticketHistory.TicketLogItemId = ticketLogItem.Id;
                        //Many to Many
                        ticket.TicketHistories.Add(ticketHistory);
                        userUpdatingTheTicket.TicketHistories.Add(ticketHistory);
                        ticketHistory.TicketId = ticket.Id;
                        ticketHistory.Ticket = ticket;
                        ticketHistory.User = userUpdatingTheTicket;
                        ticketHistory.UserId = userUpdatingTheTicket.Id;
                        ticketHistory.ChangedDate = DateTime.Now;
                        ticket.UpdatedDate = DateTime.Now;
                        //save to database
                        _ticketHistoryRepo.Add(ticketHistory);
                        _ticketLogItemRepo.Add(ticketLogItem);
                        await _userManager.UpdateAsync(userUpdatingTheTicket);//acts as db.SaveChanges()
                    }
                    return View();
                }
                catch(Exception ex)
                {
                    return NotFound("Ticket not found at UpdateTicket post method");
                }
            }
            else
            {
                return BadRequest("ticketId is null at UpdateTicket post method");
            }
        }
    }
}
