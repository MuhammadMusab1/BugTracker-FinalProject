using System.ComponentModel.DataAnnotations;

namespace BugTracker.Models
{
    public class TicketLogItem //One to One between (PrincipalTable)TicketHistory and TicketLogItem
    {
        public int Id { get; set; }
        [Display(Name = "Previous Titles")]
        public string OldTitle { get; set; }
        [Display(Name = "Previous Descriptions")]
        public string OldDescription { get; set; }
        [Display(Name = "Previous Types")]
        public string OldType { get; set; }
        [Display(Name = "Previous Priorities")]
        public string OldPriority { get; set; }
        [Display(Name = "Previous Statuses")]
        public string OldStatus { get; set; } //can only be changed by the ProjectManager of the Ticket's Project
        public TicketHistory TicketHistory { get; set; }
        public int TicketHistoryId { get; set; }
    }
}
//image clone of the old ticket