using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugTrackerTests
{
    [TestClass]
    public class TicketUnitTests
    {
        public Mock<IRepository<Ticket>> ticketRepoMock { get; set; }
        public Mock<IRepository<Project>> projectRepoMock { get; set; }
        public Mock<IRepository<TicketHistory>> ticketHistoryRepoMock { get; set; }
        public Mock<IRepository<TicketLogItem>> ticketLogItemRepoMock { get; set; }
        public Mock<UserManager<ApplicationUser>> userManager { get; set; }
        public TicketBusinessLogic ticketBL { get; set; }
        [TestInitialize]
        public void Initialize()
        {
            ApplicationUser testprojectManager = new ApplicationUser
            {
                Id = "testGUID1",
                Email = "musab03@gmail.com",
                NormalizedEmail = "MUSAB03@GMAIL.COM",
                UserName = "musab03@gmail.com",
                NormalizedUserName = "MUSAB03@GMAIL.COM",
                EmailConfirmed = true,
            };
            ApplicationUser testSubmitter = new ApplicationUser
            {
                Id = "testGUID2",
                Email = "submitter03@gmail.com",
                NormalizedEmail = "SUBMITTER03@GMAIL.COM",
                UserName = "submitter03@gmail.com",
                NormalizedUserName = "SUBMITTER03@GMAIL.COM",
                EmailConfirmed = true,
            };
            userManager = new Mock<UserManager<ApplicationUser>>();
            userManager.Setup(repo => repo.FindByNameAsync("musab03@gmail.com")).ReturnsAsync(testprojectManager);
            userManager.Setup(repo => repo.FindByNameAsync("submitter03@gmail.com")).ReturnsAsync(testSubmitter);
            /*
            RepoMock = new Mock<IRepository<Account>>();
            RepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 1))).Returns( new Account
            {
                Id = 1, Name= "Savings", Balance = 100, IsActive = true
            });
             */
            ticketRepoMock = new Mock<IRepository<Ticket>>();
            projectRepoMock = new Mock<IRepository<Project>>();
            Project testProject = new Project
            {
                Id = 1,
                Name = "SeedData Project",
                Description = "Project from the SeeData class",
                ProjectManager = testprojectManager,
                ProjectManagerId = testprojectManager.Id,
                Tickets = new HashSet<Ticket>(),
            };
            projectRepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 1))).Returns(testProject);
            Ticket testTicket = new Ticket
            {
                Id = 1,
                Title = "SeedData Ticket",
                Description = "This is a ticket from the SeedData class",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Parse("June 12 2022 12:00"),
                Type = TicketType.AccountIssue,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.OnHold,
                ProjectId = testProject.Id,
                Project = testProject,
                Submitter = testSubmitter,
                SubmitterId = testSubmitter.Id,
            };
            ticketRepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 1))).Returns(testTicket);
            //ticketHistoryRepoMock.Setup(repo => repo.GetAll()).Returns(new ICollection<TicketHistory>())
        }
        [TestMethod]
        public async Task UpdateMethodWorks()
        {
            Ticket ticket = ticketRepoMock.Object.Get(1);
            List<TicketHistory> ticketHistoriesBeforeUpdate = ticketHistoryRepoMock.Object.GetAll().ToList();
            List<TicketLogItem> ticketLogItemBeforeUpdate = ticketLogItemRepoMock.Object.GetAll().ToList();
            List<ApplicationUser> applicationUsers = userManager.Object.Users.ToList();
            ticket.Title = "Updated";
            ApplicationUser user =  await userManager.Object.FindByNameAsync("submitter03@gmail.com");
            Ticket updatedTicket = await ticketBL.UpdateTicketWithTicketHistoryAndTicketLog(ticket.Id, ticket, user);

        }
    }
}