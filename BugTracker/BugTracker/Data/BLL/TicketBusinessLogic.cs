using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace BugTracker.Data.BLL
{
    public class TicketBusinessLogic
    {
        private IRepository<Project> ProjectRepo;
        private IRepository<Ticket> TicketRepo;
        private IRepository<TicketHistory> TicketHistoryRepo;
        private IRepository<TicketLogItem> TicketLogItemRepo;
        private IRepository<TicketNotification> TicketNotificationRepo;
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;

        public TicketBusinessLogic(IRepository<Project> projRepo, IRepository<Ticket> ticketRepo, IRepository<TicketHistory> ticketHistoryRepo, IRepository<TicketLogItem> ticketLogItemRepo, IRepository<TicketNotification> ticketNotificationRepo,UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            ProjectRepo = projRepo;
            TicketRepo = ticketRepo;
            UserManager = userManager;
            RoleManager = roleManager;
            TicketHistoryRepo = ticketHistoryRepo;
            TicketLogItemRepo = ticketLogItemRepo;
            TicketNotificationRepo = ticketNotificationRepo;
        }
        public async Task<Ticket> UpdateTicketWithTicketHistoryAndTicketLog(int? ticketId, Ticket updatedTicket, ApplicationUser userUpdatingTheTicket)
        {
            Ticket ticket = TicketRepo.Get(ticket => ticket.Id == ticketId);
            TicketHistory ticketHistory = new TicketHistory();
            TicketLogItem ticketLogItem = new TicketLogItem();//mirage of the old ticket
            //Title, Description, Type, status, Priority are the only properties that can be change
            //Assigning and Unassigning developer to ticket will be handled in a different method
            if (ticket.Title != updatedTicket.Title)
            {
                ticketHistory.PropertiesChanged.Add($"Title({updatedTicket.Title})"); //oldValue(newValue)
            }
            if (ticket.Description != updatedTicket.Description)
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
            if (!ticketHistory.PropertiesChanged.Any()) //no properties changed
            {
                //don't save ticketHistory, ticketLogItem and update the ticket
                //return View("Index");
                return ticket;
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

                List<TicketHistory> ticketHistoriesOfTicket = TicketHistoryRepo.GetList(ticketH => ticketH.TicketId == ticketId).ToList();
                List<TicketHistory> ticketHistoriesOfUser = TicketHistoryRepo.GetList(ticketH => ticketH.UserId == userUpdatingTheTicket.Id).ToList();
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
                TicketHistoryRepo.Add(ticketHistory);
                TicketLogItemRepo.Add(ticketLogItem);
                await UserManager.UpdateAsync(userUpdatingTheTicket);//acts as db.SaveChanges()
                return ticket;
            }
        }
        public async Task<Ticket> AssignDeveloperToTicketBL(int? ticketId, string? developerId)
        {
            ApplicationUser developer = await UserManager.FindByIdAsync(developerId);
            List<Ticket> assignTicketsOfDeveloper = TicketRepo.GetList(ticket => ticket.DeveloperId == developerId).ToList();
            Ticket ticketToAssign = TicketRepo.Get(ticket => ticket.Id == ticketId);
            //set up relationships
            ticketToAssign.DeveloperId = developer.Id;
            ticketToAssign.Developer = developer;
            developer.AssignedTickets.Add(ticketToAssign);
            //save to database
            await UserManager.UpdateAsync(developer); //acts like _db.SaveChanges()

            return ticketToAssign;
        }
        public async Task<Ticket> SendNotificationToDeveloperWhenAssignedTicket(Ticket ticket)
        {
            TicketNotification ticketNotification = new TicketNotification();
            ticketNotification.Ticket = ticket;
            ticketNotification.TicketId = ticket.Id;
            ticketNotification.User = ticket.Developer;
            ticketNotification.UserId = ticket.DeveloperId;
            ticketNotification.Message = $"{ticket.Developer.UserName}, you have been assigned to the Ticket: {ticket.Title}!!";
            TicketNotificationRepo.GetList(ticketNot => ticketNot.TicketId == ticket.Id).ToList();
            TicketNotificationRepo.GetList(ticketNot => ticketNot.UserId == ticket.DeveloperId).ToList();
            ticket.TicketNotifications.Add(ticketNotification);
            ticket.Developer.TicketNotifications.Add(ticketNotification);
            TicketNotificationRepo.Add(ticketNotification);
            await UserManager.UpdateAsync(ticket.Developer);

            return ticket;
        }
    }
}
