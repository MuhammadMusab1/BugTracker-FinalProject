using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Data.BLL
{
    public class TicketBusinessLogic
    {
        private IRepository<Project> ProjectRepo;
        private IRepository<Ticket> TicketRepo;
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;

        public TicketBusinessLogic(IRepository<Project> projRepo, IRepository<Ticket> ticketRepo, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            ProjectRepo = projRepo;
            TicketRepo = ticketRepo;
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public List<Ticket> GetAllTickets()
        {
            return TicketRepo.GetAll().ToList();
        }

        public List<Ticket> GetAllTicketsFromUser(string userId)
        {
            return TicketRepo.GetList(t => t.SubmitterId == userId).ToList();
        }
    }
}
