﻿namespace BugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int ProjectId { get; set; }
        public string DeveloperId { get; set; }
        public ApplicationUser Developer { get; set; }
        public string SubmitterId { get; set; }
        public ApplicationUser Submitter { get; set; }
    }

    public enum TicketPriority
    {
        High,
        Medium,
        Low
    };

    public enum TicketType
    {
        CrashReport,
        AccountIssue,
        GeneralQuestion
    };

    public enum TicketStatus
    {
        Unopened,
        Opened,
        OnHold,
        Completed
    }
}
