namespace BugTracker.Models
{
    public class TicketAttachment //breakTable between ApplicationUser and Ticket
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string SubmitterId { get; set; }
        public ApplicationUser Submitter { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; } // ".pdf", ".jpg" etc
        public byte[] FileInBytes { get; set; } //will be converted to DataType: varbinary(MAX) this is how we will store our files on the database
        public DateTime CreatedDate { get; set; }
    }
}
