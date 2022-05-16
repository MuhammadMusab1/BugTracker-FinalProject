namespace BugTracker.Models
{
    public class TicketLogItem //One to One between (PrincipalTable)TicketHistory and TicketLogItem
    {
        public int Id { get; set; }
        public string OldTitle { get; set; }
        public string OldDescription { get; set; }
        public string OldType { get; set; }
        public string OldPriority { get; set; }
        public string OldStatus { get; set; } //can only be changed by the ProjectManager of the Ticket's Project
        public TicketHistory TicketHistory { get; set; }
        public int TicketHistoryId { get; set; }
    }
}
//image clone of the old ticket