using BugTracker.Data.BLL;
using BugTracker.Data.DAL;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
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
        public Mock<IRepository<TicketComment>> ticketCommentRepoMock { get; set; }
        public Mock<IRepository<TicketAttachment>> ticketAttachmentRepoMock { get; set; }
        public Mock<UserManager<ApplicationUser>> userManagerMock { get; set; }
        public Mock<RoleManager<IdentityRole>> roleManagerMock { get; set; }
        public TicketBusinessLogic ticketBL { get; set; }
        public CommentAndAttachmentBusinessLogic commentAndAttachmentBL { get; set; }
        public ProjectBusinessLogic projectBL { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            //Instantiate repos
            ticketRepoMock = new Mock<IRepository<Ticket>>();
            projectRepoMock = new Mock<IRepository<Project>>();
            ticketHistoryRepoMock = new Mock<IRepository<TicketHistory>>();
            ticketLogItemRepoMock = new Mock<IRepository<TicketLogItem>>();
            ticketNotificationRepoMock = new Mock<IRepository<TicketNotification>>();
            ticketCommentRepoMock = new Mock<IRepository<TicketComment>>();
            ticketAttachmentRepoMock = new Mock<IRepository<TicketAttachment>>();
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
                Developers = new HashSet<ApplicationUser>(),
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
            commentAndAttachmentBL = new CommentAndAttachmentBusinessLogic(projectRepoMock.Object, ticketRepoMock.Object, ticketAttachmentRepoMock.Object, ticketCommentRepoMock.Object, userManagerMock.Object);
            projectBL = new ProjectBusinessLogic(projectRepoMock.Object, ticketRepoMock.Object, userManagerMock.Object, roleManagerMock.Object);
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

        [TestMethod]
        public async Task AddingCommentToTicketWorks()
        {
            //arrange
            ApplicationUser developer = await userManagerMock.Object.FindByIdAsync("testGUID3");
            Ticket ticket = new Ticket()
            {
                Id = 1,
                Title = "test comment",
                Description = "test comment",
                Developer = developer,
                DeveloperId = developer.Id,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Parse("June 12 2022 12:00"),
                Type = TicketType.AccountIssue,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.OnHold,           
            };

            TicketComment ticketComment = new TicketComment()
            {
                Comment = "test comment",
                CreatedDate = DateTime.Now,
                Ticket = ticket,
                TicketId = ticket.Id,
                User = developer,
                UserId = developer.Id,
            };

            List<TicketComment> allTicketComments = new List<TicketComment>();
            ticketCommentRepoMock.Setup(repo => repo.Get(It.IsAny<Func<TicketComment, bool>>())).Returns(ticketComment);
            ticketCommentRepoMock.Setup(repo => repo.Add(It.IsAny<TicketComment>())).Callback<TicketComment>((ticketCom) => allTicketComments.Add(ticketCom));

            //act
            commentAndAttachmentBL.AddCommentToTicket(ticket.Id, ticketComment.Comment, developer);

            //assert
            Assert.AreEqual(1, allTicketComments.Count);
        }

        [TestMethod]
        public async Task TicketAttachmentAddWorks()
        {
            //arrange
            ApplicationUser developer = await userManagerMock.Object.FindByIdAsync("testGUID3");
            Ticket ticket = new Ticket()
            {
                Id = 1,
                Title = "test attachment",
                Description = "test attachment",
                Developer = developer,
                DeveloperId = developer.Id,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Parse("June 12 2022 12:00"),
                Type = TicketType.AccountIssue,
                Priority = TicketPriority.Medium,
                Status = TicketStatus.OnHold,
            };

            TicketAttachment ticketAttachment = new TicketAttachment()
            {
                CreatedDate = DateTime.Now,
                SubmitterId = developer.Id,
                Submitter = developer,
                TicketId = ticket.Id,
                Ticket = ticket,
                FileName = "test.txt",
                FileInBytes = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}/BugTracker.dll")
            };

            List<TicketAttachment> allTicketAttachments = new List<TicketAttachment>();
            ticketAttachmentRepoMock.Setup(repo => repo.Get(It.IsAny<Func<TicketAttachment, bool>>())).Returns(ticketAttachment);
            ticketAttachmentRepoMock.Setup(repo => repo.Add(It.IsAny<TicketAttachment>())).Callback<TicketAttachment>((ticketAttach) => allTicketAttachments.Add(ticketAttach));

            //act
            commentAndAttachmentBL.AddAttachmentToTicket(ticketAttachment.FileName, $"{Directory.GetCurrentDirectory()}/BugTracker.dll", ticket, developer);

            //assert
            Assert.AreEqual(1, allTicketAttachments.Count);
        }

        [TestMethod]
        public async Task EditCommentWorks()
        {
            //arrange
            TicketComment actualComment = new TicketComment()
            {
                Id = 1,
                TicketId = 1,
                Comment = "test comment"
            };

            TicketComment expectedComment = new TicketComment()
            {
                Id = 1,
                TicketId = 1,
                Comment = "Edited comment"
            };

            ticketCommentRepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 1))).Returns(actualComment);

            //act
            commentAndAttachmentBL.EditCommentOnTicket(actualComment.Id, "Edited comment");

            //assert
            Assert.AreNotEqual(expectedComment, actualComment);
        }

        [TestMethod]
        public async Task AddDevToProjectBLLMethodWorks()
        {
            
            //Arrange
            ApplicationUser developer = await userManagerMock.Object.FindByIdAsync("testGUID3");
            Project actualProject = new Project()
            {
                Id = 2,
                Name = "SeedData Project Testing",
                Description = "A Test Project",
                Developers = new List<ApplicationUser>(),
            };
                   
          
            projectRepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 2))).Returns(actualProject);


            //Act
            await projectBL.AddDeveloperToProject(developer.Id, actualProject.Id);

            //Assert
            Assert.AreEqual(1, actualProject.Developers.Count);
            Assert.IsNotNull(developer.ProjectAssigned);
            Assert.IsNotNull(developer.ProjectAssignedId);
        }

        [TestMethod]

        public async Task AddPMtoProjectBllMethod()
        {
            ApplicationUser developer = await userManagerMock.Object.FindByIdAsync("testGUID1");
            Project project = new Project()
            {
                Id = 2,
                Name = "SeedData Project Testing",
                Description = "A Test Project",
                Tickets = new HashSet<Ticket>(),
                Developers = new HashSet<ApplicationUser>(),

            };
            projectRepoMock.Setup(repo => repo.Get(It.Is<int>(i => i == 2))).Returns(project);
            projectRepoMock.Setup(repo => repo.GetList(It.IsAny<Func<Project, bool>>())).Returns(new List<Project>());

            //Act
            projectBL.AssignProjToPM(project.Id, developer.Id);

            //Assert
            Assert.IsNotNull(project.ProjectManager);
            Assert.IsNotNull(project.ProjectManagerId);
            Assert.AreEqual(1, developer.ProjectsOwned.Count);
        }
    }
}