using BugTracker.Data;
using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private ApplicationDbContext _db { get; set; }
        private ProjectRepository _projectRepo { get; set; }
        private TicketRepository _ticketRepo { get; set; }
        private TicketHistoryRepository _ticketHistoryRepo { get; set; }
        private TicketLogItemRepository _ticketLogItemRepo { get; set; }
        private TicketAttachmentRepository _ticketAttachmentRepo { get; set; }
        private TicketCommentRepository _ticketCommentRepo { get; set; }
        private TicketNotificationRepository _ticketNotificationRepo { get; set; }
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
            _ticketNotificationRepo = new TicketNotificationRepository(Db);
            ticketBL = new TicketBusinessLogic(_projectRepo, _ticketRepo, _ticketHistoryRepo, _ticketLogItemRepo, _ticketNotificationRepo,_userManager, _roleManager);
            commentattachmentBL = new CommentAndAttachmentBusinessLogic(_projectRepo, _ticketRepo, _ticketAttachmentRepo, _ticketCommentRepo, _userManager);
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ProjectTickets(int projectId, int? page)
        {
            //Pages for pagination
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
            ViewBag.ProjectId = projectId;
            IPagedList<Ticket> projectTickets = _ticketRepo.GetList(t => t.ProjectId == projectId).ToPagedList(pageNumber, pageSize);
            Project project = _projectRepo.Get(projectId);
            foreach(Ticket ticket in projectTickets)//to query submitters and developer from the database
            {
                ApplicationUser submitter = await _userManager.FindByIdAsync(ticket.SubmitterId);
                ApplicationUser developer = await _userManager.FindByIdAsync(ticket.DeveloperId);
            }
            return View(projectTickets);
        }

        [Authorize(Roles = "Submitter")]
        //https://localhost:7045/ticket/createTicket
        [HttpGet]
        public async Task<IActionResult> CreateTicket(int projectId)
        {
            ApplicationUser submitter = await _userManager.FindByEmailAsync(User.Identity.Name);
            ViewBag.SubmitterId = submitter.Id;
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
                return RedirectToAction("ProjectTickets", new { projectId = project.Id });
            }
            return View();
        }

        [Authorize(Roles = "Developer, Project Manager")]
        //https://localhost:7045/ticket/updateTicket?ticketId=5
        [Authorize(Roles ="Submitter")]
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
                    return RedirectToAction("TicketDetails", new {ticketId = ticket.Id});
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

        [Authorize(Roles = "Project Manager")]
        //https://localhost:7045/ticket/assignDeveloperToTicket
        [HttpGet]
        public async Task<IActionResult> AssignDeveloperToTicket(int? ticketId) //parameter: int? ticketId
        {
            if(ticketId != null)
            {
                List<ApplicationUser> developers = new List<ApplicationUser>(await _userManager.GetUsersInRoleAsync("Developer"));
                ViewBag.developerList = new SelectList(developers, "Id", "UserName");
                ViewBag.ticketId = ticketId;
                return View();
            }
            else
            {
                return NotFound("ticketId is null at AssignDeveloperToTicket get method");
            }
        }

        //https://localhost:7045/ticket/assignDeveloperToTicket?ticketId=5&&developerId=234
        [HttpPost]
        public async Task<IActionResult> AssignDeveloperToTicket(int? ticketId, string? developerId)
        {
            if(ticketId != null && developerId != null)
            {
                try
                {
                    Ticket ticket = await ticketBL.AssignDeveloperToTicketBL(ticketId, developerId);
                    await ticketBL.SendNotificationToDeveloperWhenAssignedTicket(ticket);
                    return RedirectToAction("TicketDetails", new {ticketId = ticketId});
                }
                catch (Exception ex)
                {
                    return NotFound("ticket notFound at AssignDeveloperToTicket post method");
                }
            }
            else
            {
                return BadRequest("ticketId or developerId is missing at AssignDeveloperToTicket post method");
            }
        }

        public async Task<IActionResult> DeleteTicket(int ticketId)
        {
            return View();
        }

        public async Task<IActionResult> TicketDetails(int ticketId)
        {
            ApplicationUser currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            if (await _userManager.IsInRoleAsync(currentUser, "Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
            Ticket ticket = _ticketRepo.Get(ticketId);
            //Query things from Database (works like include)
            _projectRepo.Get(ticket.ProjectId);
            await _userManager.FindByIdAsync(ticket.SubmitterId);
            await _userManager.FindByIdAsync(ticket.DeveloperId);
            List<TicketComment> CommentList = _ticketCommentRepo.GetList(ticketComment => ticketComment.TicketId == ticketId).ToList();
            foreach (TicketComment ticketComment in CommentList)
            {
                await _userManager.FindByIdAsync(ticketComment.UserId);
            }
            ViewBag.CommentList = CommentList;
            return View(_ticketRepo.Get(ticketId));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> AllTicketSort(string? filterId, int? page)
        {
            //Pages for pagination
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            IPagedList<Ticket> allTickets;
            try
            {
                if (filterId == "0")
                {
                    return RedirectToAction("SortByTitle");
                } else if (filterId == "1")
                {
                    return RedirectToAction("SortBySubmitter");
                }
                else if (filterId == "2")
                {
                    return RedirectToAction("SortByDeveloper");
                }
                else if (filterId == "3")
                {
                    return RedirectToAction("SortByCreateDate");
                } 
                else if (filterId == "4")
                {
                    return RedirectToAction("SortByTicketType");
                }
                else if (filterId == "5")
                {
                    return RedirectToAction("SortByPriority");
                }
                else if (filterId == "6")
                {
                    return RedirectToAction("SortByTicketStatus");
                }
                else if (filterId == "7")
                {
                    return RedirectToAction("SortByTicketProject");
                } 
                else
                {
                    allTickets = _ticketRepo.GetAll().ToPagedList(pageNumber, pageSize);
                    foreach (Ticket ticket in allTickets) //quering data for ticket(Like include)
                    {
                        await _userManager.FindByIdAsync(ticket.DeveloperId);
                        await _userManager.FindByIdAsync(ticket.SubmitterId);
                        _projectRepo.Get(ticket.ProjectId);
                    };
                    ViewBag.actionName = "AllTicketSort";
                    return View(allTickets);
                }
            }
            catch (Exception ex)
            {
                return NotFound("Something went wrong... Please try again");
            }
        }
        #region SortingActions(Methods)
        public async Task<IActionResult> SortByTitle(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortByTitle";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.Title).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        public async Task<IActionResult> SortBySubmitter(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortBySubmitter";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.SubmitterId).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        public async Task<IActionResult> SortByDeveloper(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortByDeveloper";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.DeveloperId).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        public async Task<IActionResult> SortByCreateDate(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortByCreateDate";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.CreatedDate).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        public async Task<IActionResult> SortByTicketType(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortByTicketType";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.Type).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        public async Task<IActionResult> SortByPriority(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortByPriority";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.Priority).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        public async Task<IActionResult> SortByTicketStatus(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortByTicketStatus";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.Status).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        public async Task<IActionResult> SortByTicketProject(int? page)
        {
            int pageNumber = page ?? 1;
            int pageSize = 10;  //hardcode how many records will be displaying on 1 page
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
            ViewBag.actionName = "SortByTicketProject";
            IPagedList<Ticket> allTickets = _ticketRepo.GetAll().OrderBy(u => u.ProjectId).ToPagedList(pageNumber, pageSize);
            foreach (Ticket ticket in allTickets)
            {
                await _userManager.FindByIdAsync(ticket.DeveloperId);
                await _userManager.FindByIdAsync(ticket.SubmitterId);
                _projectRepo.Get(ticket.ProjectId);
            }
            return View("AllTicketSort", allTickets);
        }
        #endregion
        [HttpPost]
        public async Task<IActionResult> TicketDetails(int ticketId, string comment)
        {
            ApplicationUser userCommenting = await _userManager.FindByNameAsync(User.Identity.Name);
            commentattachmentBL.AddCommentToTicket(ticketId, comment, userCommenting);
            _ticketCommentRepo.Save();
            List<TicketComment> CommentList = _ticketCommentRepo.GetList(ticketComment => ticketComment.TicketId == ticketId).ToList();
            foreach(TicketComment ticketComment in CommentList)
            {
                await _userManager.FindByIdAsync(ticketComment.UserId);
            }
            ViewBag.CommentList = CommentList;
            if(await _userManager.IsInRoleAsync(userCommenting, "Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
            Ticket ticket = _ticketRepo.Get(ticketId);
            _projectRepo.Get(ticket.ProjectId); //query the project
            return View(ticket);

        }

        [Authorize(Roles = "Developer")]
        public IActionResult ListDeveloperTickets()
        {
            return View(_ticketRepo.GetList(d => d.Developer == User.Identity));
        }
    }
}
