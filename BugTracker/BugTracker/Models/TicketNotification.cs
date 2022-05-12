namespace BugTracker.Models
{
    public class TicketNotification //breakTable between Ticket and ApplicationUser
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public ApplicationUser User { get; set; }
        public string UserId { get; set; }
    }
}
