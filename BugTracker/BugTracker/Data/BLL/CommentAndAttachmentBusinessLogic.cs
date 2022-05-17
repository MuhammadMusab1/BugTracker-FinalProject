using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace BugTracker.Data.BLL
{
    public class CommentAndAttachmentBusinessLogic
    {
        private IRepository<Project> ProjectRepo;
        private IRepository<Ticket> TicketRepo;
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;

        public CommentAndAttachmentBusinessLogic(IRepository<Project> projRepo, IRepository<Ticket> ticketRepo, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            ProjectRepo = projRepo;
            TicketRepo = ticketRepo;
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public async void AddCommentToTicket(int ticketId, string userId, TicketComment comment)
        {
            Ticket ticket = TicketRepo.Get(ticketId);
            //ApplicationUser user = await UserManager.FindByIdAsync(userId);
            //bool HasPerms = await UserManager.IsInRoleAsync(user, "Admin");
            //if (ticket.Submitter.Id == userId || ticket.DeveloperId == userId || HasPerms == true)
            ticket.TicketComments.Add(comment);
        }

        public void AddAttachmentToTicket(string fileName, string filePath, string userId, int ticketId)
        {
            TicketAttachment ticketAttachment = new TicketAttachment()
            {
                CreatedDate = DateTime.Now,
                FileInBytes = File.ReadAllBytes(filePath),
                SubmitterId = userId,
                TicketId = ticketId,
                FileName = fileName,
                FileExtension = Path.GetExtension(filePath)
            };
        }

        public void EditCommentOnTicket()
        {

        }
    }
}
