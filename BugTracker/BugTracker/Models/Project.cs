using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Display(Name = "Project Manager")]
        public ApplicationUser ProjectManager { get; set; }
        public string ProjectManagerId { get; set; }
        public ICollection<Ticket>? Tickets { get; set; }
        public ICollection<ApplicationUser>? Developers { get; set; }
        public Project()
        {
            Developers = new HashSet<ApplicationUser>();
            Tickets = new HashSet<Ticket>();
        }
    }
}
