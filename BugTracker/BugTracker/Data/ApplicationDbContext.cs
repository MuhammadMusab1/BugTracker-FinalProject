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


            //Multiple One to Many between ApplicationUser and Ticket
            builder.Entity<Ticket>()
                .HasOne(assignedTicket => assignedTicket.Developer)
                .WithMany(developer => developer.AssignedTickets)
                .HasForeignKey(ticket => ticket.DeveloperId); //foreignKey stays on the Many part of the relationship

            builder.Entity<Ticket>()
                .HasOne(submittedTicket => submittedTicket.Submitter)
                .WithMany(submitter => submitter.SubmittedTickets)
                .HasForeignKey(ticket => ticket.SubmitterId); //foreignKey stays on the Many part of the relationship

            //Only Developer can be assigned to Tickets (No one else can be assigned unless they are also a Developer).

            //TickHistory: breakTable between Ticket and ApplicationUser
            //One to Many between Ticket and TicketHistory
            builder.Entity<TicketHistory>()
                .HasOne(ticketHistory => ticketHistory.Ticket)
                .WithMany(ticket => ticket.TicketHistories)
                .HasForeignKey(ticketHistory => ticketHistory.TicketId); 
            //One to Many between ApplicationUser and TicketHistory
            builder.Entity<TicketHistory>()
                .HasOne(ticketHistory => ticketHistory.User)
                .WithMany(user => user.TicketHistories)
                .HasForeignKey(ticketHistory => ticketHistory.UserId);

            //TickComment: breakTable between Ticket and ApplicationUser
            //One to Many between Ticket and TicketComment
            builder.Entity<TicketComment>()
                .HasOne(ticketHistory => ticketHistory.Ticket)
                .WithMany(ticket => ticket.TicketComments)
                .HasForeignKey(ticketHistory => ticketHistory.TicketId);
            //One to Many between ApplicationUser and TicketComment
            builder.Entity<TicketComment>()
                .HasOne(ticketHistory => ticketHistory.User)
                .WithMany(user => user.TicketComments)
                .HasForeignKey(ticketHistory => ticketHistory.UserId);

        }
    }
}