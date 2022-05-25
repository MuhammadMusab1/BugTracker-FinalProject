using BugTracker.Data;
using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private TicketAttachmentRepository _ticketAttachmentRepo { get; set; }
        private TicketCommentRepository _ticketCommentRepo { get; set; }
        private CommentAndAttachmentBusinessLogic commentattachmentBL { get; set; }
        private TicketBusinessLogic ticketBL { get; set; }
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
            _ticketAttachmentRepo = new TicketAttachmentRepository(Db);
            _ticketCommentRepo = new TicketCommentRepository(Db);
            ticketBL = new TicketBusinessLogic(_projectRepo, _ticketRepo, _ticketHistoryRepo, _ticketLogItemRepo, _userManager, _roleManager);
            commentattachmentBL = new CommentAndAttachmentBusinessLogic(_projectRepo, _ticketRepo, _ticketAttachmentRepo, _ticketCommentRepo, _userManager);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ProjectTickets(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View(_ticketRepo.GetList(t => t.ProjectId == projectId));
        }

        //https://localhost:7045/ticket/createTicket
        [HttpGet]
        public IActionResult CreateTicket(int projectId)
        {
            string userName = User.Identity.Name;
            ViewBag.SubmitterId = _userManager.FindByEmailAsync(userName);
            ViewBag.ProjectId = projectId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTicket([Bind("Title, Description, CreatedDate, UpdatedDate, Priority, Status, Type, SubmitterId, ProjectId")] Ticket newTicket, List<IFormFile> files)
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
                if (files != null)
                {
                    var size = files.Sum(f => f.Length);
                    var filePaths = new List<string>();

                    foreach (var formFile in files)
                    {
                        if (formFile.Length > 0)
                        {
                            var filePath = Path.Combine($"{Directory.GetCurrentDirectory()}/UploadedFiles", formFile.FileName);
                            filePaths.Add(filePath);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await formFile.CopyToAsync(stream);
                            }

                            commentattachmentBL.AddAttachmentToTicket(formFile.FileName, filePath, newTicket, submitter);
                        }
                    }
                }
                submitter.SubmittedTickets.Add(newTicket);
                project.Tickets.Add(newTicket);
                _ticketRepo.Add(newTicket);
                await _userManager.UpdateAsync(submitter); //since we used _userManager in this method we can't use _ticketRepo.Save()
                return RedirectToAction("ProjectTickets", project.Id);
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
                    Ticket ticket = await ticketBL.UpdateTicketWithTicketHistoryAndTicketLog(ticketId, updatedTicket, userUpdatingTheTicket);
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

        public async Task<IActionResult> DeleteTicket(int ticketId)
        {
            return View();
        }

        public async Task<IActionResult> TicketDetails(int ticketId)
        {
            return View(_ticketRepo.Get(ticketId));
        }

        [HttpGet]
        public IActionResult AllTicketSort(string? filterId)
        {
            ViewBag.sortList = new List<SelectListItem>
            {
                new SelectListItem("Title", "0"),
                new SelectListItem("Owner", "1"),
                new SelectListItem("Assignment", "2"),
                new SelectListItem("Creation Time", "3"),
                new SelectListItem("Ticket Type", "4"),
                new SelectListItem("Priority", "5"),
                new SelectListItem("Status", "6"),
                new SelectListItem("Project", "7")
            };

            try
            {
                if (filterId == "0")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.Title));
                }
                if (filterId == "1")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.SubmitterId));
                }
                if (filterId == "2")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.DeveloperId));
                }
                if (filterId == "3")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.CreatedDate));
                }
                if (filterId == "4")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.Type));
                }
                if (filterId == "5")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.Priority));
                }
                if (filterId == "6")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.Status));
                }
                if (filterId == "7")
                {
                    return View(_ticketRepo.GetAll().OrderBy(u => u.ProjectId));
                }
            }
            catch (Exception ex)
            {
                return NotFound("Something went wrong... Please try again");
            }
            return View(_ticketRepo.GetAll());  
        }
    }
}
