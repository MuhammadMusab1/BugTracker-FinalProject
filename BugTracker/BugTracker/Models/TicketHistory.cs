namespace BugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public ICollection<string> Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public bool Changed { get; set; }
        public string UserId { get; set; }
    }
}
