using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class TicketHistory //BreakTable between Ticket and User 
    {
        public int Id { get; set; }
        [NotMapped]
        public ICollection<string> PropertiesChanged { get; set; }
        public DateTime ChangedDate { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } //Changed By 
        public TicketLogItem? TicketLogItem { get; set; }
        public int? TicketLogItemId { get; set; }
        public TicketHistory()
        {
            PropertiesChanged = new HashSet<string>();
        }
    }
    //See who can Edit what from the pic
    //file:///C:/Users/MUHAMM~1/AppData/Local/Temp/Rar$DIa31952.30828/BugTrackerPermissionsByRoles.pdf
}
