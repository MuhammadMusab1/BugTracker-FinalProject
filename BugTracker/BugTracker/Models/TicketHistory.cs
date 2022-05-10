namespace BugTracker.Models
{
    public class TicketHistory //BreakTable between Ticket and User 
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string Property { get; set; } //the property of the Ticket object that is changed
        public string OldValue { get; set; } //the original value of the changed property
        public string NewValue { get; set; } // the new value of the property
        public DateTime ChangedDate { get; set; } //the date/time of when this value was changed
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } //Changed By 
    }
    //See who can Edit what from the pic
    //file:///C:/Users/MUHAMM~1/AppData/Local/Temp/Rar$DIa31952.30828/BugTrackerPermissionsByRoles.pdf
}
