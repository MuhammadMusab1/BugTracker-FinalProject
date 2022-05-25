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
        public Mock<IRepository<TicketNotification>> ticketNotificationRepoMock { get; set; }
        public Mock<UserManager<ApplicationUser>> userManagerMock { get; set; }
        public Mock<RoleManager<IdentityRole>> roleManagerMock { get; set; }
        public TicketBusinessLogic ticketBL { get; set; }
        [TestInitialize]
        public void Initialize()
        {
            //Instantiate repos
            ticketRepoMock = new Mock<IRepository<Ticket>>();
            projectRepoMock = new Mock<IRepository<Project>>();
            ticketHistoryRepoMock = new Mock<IRepository<TicketHistory>>();
            ticketLogItemRepoMock = new Mock<IRepository<TicketLogItem>>();
            ticketNotificationRepoMock = new Mock<IRepository<TicketNotification>>();
            //stackoverflow solution(mine was giving errors)
            roleManagerMock = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            userManagerMock = new Mock<UserManager<ApplicationUser>>(Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

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
            ApplicationUser testDeveloper = new ApplicationUser
            {
                Id = "testGUID3",
                Email = "developer03@gmail.com",
                NormalizedEmail = "DEVELOPER03@GMAIL.COM",
                UserName = "developer03@gmail.com",
                NormalizedUserName = "DEVELOPER03@GMAIL.COM",
                EmailConfirmed = true,
            };
            List<ApplicationUser> allUsers = new List<ApplicationUser>
            {
                testprojectManager, testSubmitter, testDeveloper
            };

            Project testProject = new Project
            {
                Id = 1,
                Name = "SeedData Project",
                Description = "Project from the SeeData class",
                ProjectManager = testprojectManager,
                ProjectManagerId = testprojectManager.Id,
                Tickets = new HashSet<Ticket>(),
            };
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
            //ProjectRepo Setup
            projectRepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 1))).Returns(testProject);
            testProject.Tickets.Add(testTicket);
            //TicketRepo Setup
            ticketRepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 1))).Returns(testTicket);
            ticketRepoMock.Setup(repo => repo.Get(It.IsAny<Func<Ticket, bool>>())).Returns(testTicket);
            //UserManager Setup
            userManagerMock.Setup(repo => repo.FindByNameAsync("musab03@gmail.com")).ReturnsAsync(testprojectManager);
            userManagerMock.Setup(repo => repo.FindByNameAsync("submitter03@gmail.com")).ReturnsAsync(testSubmitter);
            userManagerMock.Setup(repo => repo.FindByNameAsync("developer03@gmail.com")).ReturnsAsync(testDeveloper);
            userManagerMock.Setup(repo => repo.FindByIdAsync("testGUID1")).ReturnsAsync(testprojectManager);
            userManagerMock.Setup(repo => repo.FindByIdAsync("testGUID2")).ReturnsAsync(testSubmitter);
            userManagerMock.Setup(repo => repo.FindByIdAsync("testGUID3")).ReturnsAsync(testDeveloper);
            userManagerMock.Setup(repo => repo.Users).Returns(allUsers.AsQueryable);
            //TicketHistoryRepo Setup
            ticketHistoryRepoMock.Setup(repo => repo.GetAll()).Returns(new HashSet<TicketHistory>());
            //TicketLogItem Setup
            ticketLogItemRepoMock.Setup(repo => repo.GetAll()).Returns(new HashSet<TicketLogItem>());

            //Instantiate TicketBL
            ticketBL = new TicketBusinessLogic(projectRepoMock.Object, ticketRepoMock.Object, ticketHistoryRepoMock.Object, ticketLogItemRepoMock.Object, ticketNotificationRepoMock.Object, userManagerMock.Object, roleManagerMock.Object);
        }
        [TestMethod]
        public async Task UpdateMethodUpdatesTheTicketAndCreateTicketHistoryAndTicketLogForIt()
        {
            //Arrange
            Ticket ticket = ticketRepoMock.Object.Get(1);
            int ticketHistoriesExpectedCount = ticketHistoryRepoMock.Object.GetAll().ToList().Count() + 1;
            int ticketLogItemsExpectedCount = ticketLogItemRepoMock.Object.GetAll().ToList().Count() + 1;

            List<TicketHistory> ticketHistories = new List<TicketHistory>();
            List<TicketLogItem> ticketLogItems = new List<TicketLogItem>();

            ticketHistoryRepoMock.Setup(repo => repo.GetList(It.IsAny<Func<TicketHistory, bool>>())).Returns(ticketHistories);// returns an empty List ticketHistory just for this method

            ticketHistoryRepoMock.Setup(repo => repo.Add(It.IsAny<TicketHistory>())).Callback<TicketHistory>((tH) => ticketHistories.Add(tH));
            ticketLogItemRepoMock.Setup(repo => repo.Add(It.IsAny<TicketLogItem>())).Callback<TicketLogItem>((tLI) => ticketLogItems.Add(tLI));
            //Act
            Ticket updatedTicket = new Ticket() //what the updatedTicket would look like
            {
                Title = "Updated the Ticket",
                Description = "This is a ticket from the SeedData class",
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Parse("June 12 2022 12:00"),
                Type = TicketType.AccountIssue,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.OnHold,
            };
            ApplicationUser userUpdatingTheTicket =  await userManagerMock.Object.FindByNameAsync("submitter03@gmail.com");
            await ticketBL.UpdateTicketWithTicketHistoryAndTicketLog(ticket.Id, updatedTicket, userUpdatingTheTicket);

            //ASSERT
            int ticketHistoriesActualCount = ticketHistories.Count();
            int ticketLogItemsActualCount = ticketLogItems.Count();

            Assert.AreEqual(ticketHistoriesExpectedCount, ticketHistoriesActualCount);
            Assert.AreEqual(ticketLogItemsExpectedCount, ticketLogItemsActualCount);

        }
        [TestMethod]
        public async Task AssignDeveloperToTicketBLLMethodWorks()
        {
            //Arrange
            Ticket ticket = ticketRepoMock.Object.Get(1);
            ticketRepoMock.Setup(repo => repo.GetList(It.IsAny<Func<Ticket, bool>>())).Returns(new HashSet<Ticket>()); //an empty ListOfAssignedTickets for developer
            ApplicationUser developer = await userManagerMock.Object.FindByNameAsync("developer03@gmail.com");
            //Act
            await ticketBL.AssignDeveloperToTicketBL(ticket.Id, developer.Id);
            //Assert
            Assert.IsNotNull(ticket.Developer);
            Assert.IsNotNull(ticket.DeveloperId);
            Assert.AreEqual(1, developer.AssignedTickets.Count()); //after add a ticket to List
        }
        [TestMethod]
        public async Task SendNotificationToDeveloperWhenAssignedTicketWorks()
        {
            //Arrange
            ApplicationUser developer = await userManagerMock.Object.FindByIdAsync("testGUID3");
            Ticket ticket = new Ticket
            {
                Id = 3,
                Title = "Testing Notification",
                Description = "This ticket is for only testing notifications",
                Developer = developer,
                DeveloperId = developer.Id,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Parse("June 12 2022 12:00"),
                Type = TicketType.AccountIssue,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.OnHold,
            };
            developer.AssignedTickets = new HashSet<Ticket>();
            developer.AssignedTickets.Add(ticket);
            List<TicketNotification> allNotifications = new List<TicketNotification>();
            ticketNotificationRepoMock.Setup(repo => repo.GetList(It.IsAny<Func<TicketNotification, bool>>())).Returns(new HashSet<TicketNotification>());
            ticketNotificationRepoMock.Setup(repo => repo.Add(It.IsAny<TicketNotification>())).Callback<TicketNotification>((ticketNot) => allNotifications.Add(ticketNot));
            //Act 
            await ticketBL.SendNotificationToDeveloperWhenAssignedTicket(ticket);
            //Assert
            Assert.AreEqual(1, ticket.TicketNotifications.Count());
            Assert.AreEqual(1, developer.TicketNotifications.Count());
            Assert.AreEqual(1, allNotifications.Count());
        }
    }
}