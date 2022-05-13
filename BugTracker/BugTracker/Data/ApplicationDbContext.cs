using BugTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //PrimaryKey set up
            builder.Entity<ApplicationUser>().HasKey(user => user.Id);
            builder.Entity<Ticket>().HasKey(ticket => ticket.Id);
            builder.Entity<TicketHistory>().HasKey(ticketHistory => ticketHistory.Id);
            builder.Entity<TicketComment>().HasKey(ticketComment => ticketComment.Id);
            builder.Entity<TicketNotification>().HasKey(ticketNotification => ticketNotification.Id);
            builder.Entity<TicketAttachment>().HasKey(ticketAttachment => ticketAttachment.Id);
            builder.Entity<Project>().HasKey(project => project.Id);


            //Multiple One to Many between ApplicationUser and Ticket
            builder.Entity<Ticket>()
                .HasOne(assignedTicket => assignedTicket.Developer)
                .WithMany(developer => developer.AssignedTickets)
                .HasForeignKey(ticket => ticket.DeveloperId)
                .OnDelete(DeleteBehavior.NoAction);//foreignKey stays on the Many part of the relationship

            builder.Entity<Ticket>()
                .HasOne(submittedTicket => submittedTicket.Submitter)
                .WithMany(submitter => submitter.SubmittedTickets)
                .HasForeignKey(ticket => ticket.SubmitterId)
                .OnDelete(DeleteBehavior.NoAction); //foreignKey stays on the Many part of the relationship

            //Only Developer can be assigned to Tickets (No one else can be assigned unless they are also a Developer).

            //TickHistory: breakTable between Ticket and ApplicationUser
            //One to Many between Ticket and TicketHistory
            builder.Entity<TicketHistory>()
                .HasOne(ticketHistory => ticketHistory.Ticket)
                .WithMany(ticket => ticket.TicketHistories)
                .HasForeignKey(ticketHistory => ticketHistory.TicketId)
                .OnDelete(DeleteBehavior.NoAction); 
            //One to Many between ApplicationUser and TicketHistory
            builder.Entity<TicketHistory>()
                .HasOne(ticketHistory => ticketHistory.User)
                .WithMany(user => user.TicketHistories)
                .HasForeignKey(ticketHistory => ticketHistory.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //TickComment: breakTable between Ticket and ApplicationUser
            //One to Many between Ticket and TicketComment
            builder.Entity<TicketComment>()
                .HasOne(ticketHistory => ticketHistory.Ticket)
                .WithMany(ticket => ticket.TicketComments)
                .HasForeignKey(ticketHistory => ticketHistory.TicketId)
                .OnDelete(DeleteBehavior.NoAction);
            //One to Many between ApplicationUser and TicketComment
            builder.Entity<TicketComment>()
                .HasOne(ticketHistory => ticketHistory.User)
                .WithMany(user => user.TicketComments)
                .HasForeignKey(ticketHistory => ticketHistory.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //TicketNotification: breakTable between Ticket and ApplicationUser
            //One to Many between ApplicationUser and TicketComment
            builder.Entity<TicketNotification>()
                .HasOne(ticketNotification => ticketNotification.Ticket)
                .WithMany(ticket => ticket.TicketNotifications)
                .HasForeignKey(ticketHistory => ticketHistory.TicketId)
                .OnDelete(DeleteBehavior.NoAction);
            //One to Many between ApplicationUser and TicketComment
            builder.Entity<TicketNotification>()
                .HasOne(ticketNotification => ticketNotification.User)
                .WithMany(user => user.TicketNotifications)
                .HasForeignKey(ticketHistory => ticketHistory.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //TicketAttachment: breakTable between Ticket and ApplicationUser
            //One to Many between Ticket and TicketAttachment
            builder.Entity<TicketAttachment>()
                .HasOne(ticketAttachment => ticketAttachment.Ticket)
                .WithMany(ticket => ticket.TicketAttachments)
                .HasForeignKey(ticketAttachment => ticketAttachment.TicketId)
                .OnDelete(DeleteBehavior.NoAction);
            //One to Many between ApplicationUser and TicketAttachment
            builder.Entity<TicketAttachment>()
                .HasOne(ticketAttachment => ticketAttachment.Submitter)
                .WithMany(submitter => submitter.TicketAttachments)
                .HasForeignKey(ticketAttachment => ticketAttachment.SubmitterId)
                .OnDelete(DeleteBehavior.NoAction);

            //One to Many between ProjectManager and Project
            builder.Entity<Project>()
                .HasOne(project => project.ProjectManager)
                .WithMany(projectManager => projectManager.ProjectsOwned)
                .HasForeignKey(project => project.ProjectManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            //One To Many between Project and Developer
            builder.Entity<ApplicationUser>()
                .HasOne(developer => developer.ProjectAssigned)
                .WithMany(projecAssignedTo => projecAssignedTo.Developers)
                .HasForeignKey(developer => developer.ProjectAssignedId)
                .OnDelete(DeleteBehavior.NoAction);
            //One to Many between Project and Ticket
            builder.Entity<Ticket>()
                .HasOne(ticket => ticket.Project)
                .WithMany(project => project.Tickets)
                .HasForeignKey(ticket => ticket.ProjectId)
                .OnDelete(DeleteBehavior.NoAction);
        }
        public DbSet<Ticket> Ticket { get; set; }
        public DbSet<TicketComment> TicketComment { get; set; }
        public DbSet<TicketNotification> TicketNotification { get; set; }
        public DbSet<TicketHistory> TicketHistory { get; set; }
        public DbSet<TicketAttachment> TicketAttachment { get; set; }
        public DbSet<Project> Project { get; set; }
    }
}