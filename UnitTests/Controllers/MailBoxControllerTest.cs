

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using JanuszMail.Controllers;
using JanuszMail.Data;
using JanuszMail.Interfaces;
using JanuszMail.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using Moq;
using PagedList.Core;
using UnitTests.Mocks;

namespace UnitTests.Controllers
{
    [TestClass]
    public class MailBoxControllerTest
    {


        [TestInitialize]
        public void SetUp()
        {
            mockProvider = new Mock<IProvider>();

            var options = new DbContextOptionsBuilder<JanuszMailDbContext>().UseSqlite(new SqliteConnection("DataSource=:memory")).Options;
            mockDbContext = new JanuszMailDbContext(options);
            mockDbContext.Database.Migrate();
            mockUserManager = new Mock<FakeUserManager>();
            controller = new MailBoxController(mockProvider.Object, mockUserManager.Object, mockDbContext);
            fakeUser = new ApplicationUser()
            {
                Id = "user1"
            };
            mockUserManager.Setup(Manager => Manager.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(fakeUser));
        }

        //[TestMethod]
        public void GivenUserWithoutSelectedProviderWhenCallIndexMethodThenControllerReturnsErrorViewWithNoProviderMessage()
        {
            SetBadProviderParams();

            var viewResult = controller.Index().Result as ViewResult;
            Assert.AreEqual("Error", viewResult.ViewName);
            Assert.IsTrue(viewResult.ViewData["ErrorMessage"].ToString().Contains("no provider"));
        }

        [TestMethod]
        public void GivenUserWithSelectedProviderWhenCallIndexMethodThenControllerReturnsIndexView()
        {
            SetGoodProviderParams();

            var viewResult = controller.Index().Result as ViewResult;
            Assert.IsNull(viewResult.ViewName);
            Assert.IsNull(viewResult.ViewData["ErrorMessage"]);
        }

        [TestMethod]
        public void GivenProviderWhenConnectMethodReturnsOKThenControllerReturnsIndexView()
        {
            SetGoodProviderParams();
            mockProvider.Setup(mock => mock.Connect(It.IsAny<ProviderParams>())).Returns(HttpStatusCode.OK);

            var viewResult = controller.Index().Result as ViewResult;
            Assert.IsNull(viewResult.ViewName);
            Assert.IsNull(viewResult.ViewData["ErrorMessage"]);
        }

        //[TestMethod]
        public void GivenProviderWhenConnectMethodReturnsExpectationFailedThenControllerReturnsErrorViewWithFailedConnectionMessage()
        {
            SetGoodProviderParams();
            mockProvider.Setup(mock => mock.Connect(It.IsAny<ProviderParams>())).Returns(HttpStatusCode.ExpectationFailed);

            var viewResult = controller.Index().Result as ViewResult;
            Assert.AreEqual("Error", viewResult.ViewName);
            Assert.IsTrue(viewResult.ViewData["ErrorMessage"].ToString().Contains("connection failed"));
        }

        [DataTestMethod]
        [DataRow(1, 3)]
        [DataRow(2, 3)]
        [DataRow(3, 3)]
        public void GivenAuthenticatedProviderWithSomeMailsInInboxWhenShowMailsMethodIsCalledThenControllerReturnsViewWithRequestedMessages(int page, int pageSize)
        {
            SetGoodProviderParams();
            var mailList = new List<MimeMessage>();
            for (int pageId = 0; pageId < page * 2; pageId++)
            {
                for (int entryId = 0; entryId < pageSize; entryId++)
                {
                    mailList.Add(new MimeMessage()
                    {
                        MessageId = (pageId * pageSize + entryId).ToString(),
                        Date = DateTime.Now.AddDays(-(pageId * pageSize + entryId))
                    });
                }
            }


            mockProvider.Setup(mock => mock.GetMailsFromFolder(It.Is<string>(folder => folder.Equals("inbox")),
                 It.Is<int>(p => p == page), It.Is<int>(ps => ps == pageSize)))
                 .Returns(new Tuple<IList<MimeMessage>, HttpStatusCode>(mailList.Skip(pageSize * (page - 1)).Take(pageSize).ToList(), HttpStatusCode.OK));

            var viewResult = controller.ShowMails(page, pageSize, "inbox", null, null, null) as ViewResult;
            Assert.IsNull(viewResult.ViewName);
            Assert.IsNull(viewResult.ViewData["ErrorMessage"]);

            Assert.IsInstanceOfType(viewResult.Model, typeof(IPagedList<MimeMessage>));
            var receivedModel = (IPagedList<MimeMessage>)viewResult.Model;
            Assert.IsNotNull(receivedModel);
            var receivedMails = (IList<MimeMessage>)(receivedModel.ToList());
            Assert.AreEqual(pageSize, receivedMails.Count);
            for (int entryId = 0; entryId < pageSize; entryId++)
            {
                Assert.AreEqual(((page - 1) * pageSize + entryId).ToString(), receivedMails.ElementAt(entryId).MessageId);
            }

        }

        private void SetGoodProviderParams()
        {
            var fakeProviderParams = new ProviderParams()
            {
                EmailAddress = "aa@aa.pl",
                Password = "11",
                ImapServerName = "fake",
                ImapPortNumber = 1,
                SmtpServerName = "fake",
                SmtpPortNumber = 1,
                UserId = "user1"
            };
            mockDbContext.ProviderParams.Add(fakeProviderParams);
            mockDbContext.SaveChanges();
        }

        private void SetBadProviderParams()
        {
            var fakeProviderParams = new ProviderParams()
            {
                EmailAddress = "aa@aa.pl",
                Password = "11",
                ImapServerName = "fake",
                ImapPortNumber = 1,
                SmtpServerName = "fake",
                SmtpPortNumber = 1,
                UserId = "user2"
            };
            mockDbContext.ProviderParams.Add(fakeProviderParams);
            mockDbContext.SaveChanges();
        }


        Mock<IProvider> mockProvider;
        JanuszMailDbContext mockDbContext;
        MailBoxController controller;
        Mock<FakeUserManager> mockUserManager;
        ApplicationUser fakeUser;

    }
}
