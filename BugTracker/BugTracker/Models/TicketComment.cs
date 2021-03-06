namespace BugTracker.Models
{
    public class TicketComment //breakTable between Ticket and ApplicationUser
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
