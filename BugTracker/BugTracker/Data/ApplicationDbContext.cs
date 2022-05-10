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
        }
    }
}