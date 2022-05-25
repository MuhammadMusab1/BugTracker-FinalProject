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
        private IRepository<TicketAttachment> TicketAttachmentRepo;
        private IRepository<TicketComment> TicketCommentRepo;
        private UserManager<ApplicationUser> UserManager;

        public CommentAndAttachmentBusinessLogic(IRepository<Project> projRepo, IRepository<Ticket> ticketRepo, IRepository<TicketAttachment> ticketAttachmentRepo, IRepository<TicketComment> ticketCommentRepo, UserManager<ApplicationUser> userManager)
        {
            ProjectRepo = projRepo;
            TicketRepo = ticketRepo;
            TicketAttachmentRepo = ticketAttachmentRepo;
            TicketCommentRepo = ticketCommentRepo;
            UserManager = userManager;
        }

        public void AddCommentToTicket(int ticketId, string comment, ApplicationUser userCommenting)
        {
            Ticket ticket = TicketRepo.Get(ticketId);
            TicketComment ticketComment = new TicketComment()
            {
                Comment = comment,
                CreatedDate = DateTime.Now,
                Ticket = ticket,
                TicketId = ticketId,
                User = userCommenting,
                UserId = userCommenting.Id,
            };
            TicketCommentRepo.Add(ticketComment);
        }

        public void AddAttachmentToTicket(string fileName, string filePath, Ticket ticket, ApplicationUser submitter)
        {
            TicketAttachment ticketAttachment = new TicketAttachment()
            {
                CreatedDate = DateTime.Now,
                FileInBytes = File.ReadAllBytes(filePath),
                SubmitterId = submitter.Id,
                Submitter = submitter,
                TicketId = ticket.Id,
                Ticket = ticket,
                FileName = fileName,
                FileExtension = Path.GetExtension(filePath)
            };
            TicketAttachmentRepo.Add(ticketAttachment);
        }

        public void EditCommentOnTicket(TicketComment ticketComment, string newComment)
        {
            TicketComment commentToEdit = ticketComment.Ticket.TicketComments.First(tc => tc.Id == ticketComment.Id);
            commentToEdit.Comment = "";
            commentToEdit.Comment = newComment;
        }
    }
}
