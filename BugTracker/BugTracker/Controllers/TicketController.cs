using BugTracker.Data;
using BugTracker.Data.BLL;
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
        public async Task<IActionResult> CreateTicket(int projectId)
        {
            string userName = User.Identity.Name;
            ViewBag.SubmitterId = await _userManager.FindByEmailAsync(userName);
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
            ViewBag.CommentList = _db.TicketComment.Include(s => s.User).Where(c => c.TicketId == ticketId).ToList();
            //ViewBag.CommentList = _ticketCommentRepo.GetList(tc => tc.TicketId == ticketId).ToList();
            return View(_ticketRepo.Get(ticketId));
        }

        [HttpPost]
        public async Task<IActionResult> TicketDetails(int ticketId, string comment)
        {
            commentattachmentBL.AddCommentToTicket(ticketId, comment);
            _ticketCommentRepo.Save();

            ViewBag.CommentList = _db.TicketComment.Include(s => s.User).Where(c => c.TicketId == ticketId).ToList();
            //ViewBag.CommentList = _ticketCommentRepo.GetList(tc => tc.TicketId == ticketId);
            return View(_ticketRepo.Get(ticketId));
        }
    }
}
