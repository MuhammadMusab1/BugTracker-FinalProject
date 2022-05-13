using Microsoft.AspNetCore.Identity;

namespace BugTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Ticket> SubmittedTickets { get; set; }
        public ICollection<Ticket> AssignedTickets { get; set; }
        public ICollection<TicketHistory> TicketHistories { get; set; }
        public ICollection<TicketComment> TicketComments { get; set; }
        public ICollection<TicketNotification> TicketNotifications { get; set; }
        public ICollection<TicketAttachment> TicketAttachments { get; set; }
        public ICollection<Project> ProjectsOwned { get; set; }
        public Project? ProjectAssigned{ get; set; } 
        public int? ProjectAssignedId { get; set; }

        public ApplicationUser()
        {
            SubmittedTickets = new HashSet<Ticket>();
            AssignedTickets = new HashSet<Ticket>();
            TicketHistories = new HashSet<TicketHistory>();
            TicketComments = new HashSet<TicketComment>();
            TicketNotifications = new HashSet<TicketNotification>();
            TicketAttachments = new HashSet<TicketAttachment>();
        }
    }
}
