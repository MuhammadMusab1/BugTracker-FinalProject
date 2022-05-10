using Microsoft.AspNetCore.Identity;

namespace BugTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Ticket> SubmittedTickets { get; set; }
        public ICollection<Ticket> AssignedTickets { get; set; }
        public ICollection<Project> Projects { get; set; }

        public ApplicationUser()
        {
            SubmittedTickets = new HashSet<Ticket>();
            AssignedTickets = new HashSet<Ticket>();
            Projects = new HashSet<Project>();
        }
    }
}
