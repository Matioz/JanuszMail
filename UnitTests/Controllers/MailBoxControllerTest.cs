

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
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        SqliteConnection connection;

        [TestInitialize]
        public void SetUp()
        {
            mockProvider = new Mock<IProvider>();
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<JanuszMailDbContext>().UseSqlite(connection).Options;
            mockDbContext = new JanuszMailDbContext(options);
            mockDbContext.Database.Migrate();
            mockUserManager = new Mock<FakeUserManager>();
            controller = new MailBoxController(mockProvider.Object, mockUserManager.Object, mockDbContext);
            fakeUser = new ApplicationUser()
            {
                Id = "user1"
            };
            mockUserManager.Setup(Manager => Manager.GetUserAsync(It.IsAny<ClaimsPrincipal>())).Returns(Task.FromResult(fakeUser));

            Mock<IUrlHelper> urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("http://localhost");
            controller.Url = urlHelper.Object;

            controller.TempData = new TempDataDictionary(new Mock<HttpContext>().Object, new Mock<ITempDataProvider>().Object);
        }

        [TestMethod]
        public void GivenUserWithoutSelectedProviderWhenCallIndexMethodThenControllerReturnsErrorViewWithNoProviderMessage()
        {
            AddProviderParamsToOtherUser();

            var viewResult = controller.Index().Result as ViewResult;
            Assert.AreEqual("Error", viewResult.ViewName);
            Assert.IsTrue(viewResult.TempData["ErrorMessage"].ToString().Contains("no provider"));
        }

        [TestMethod]
        public void GivenUserWithSelectedProviderAndGoodConnectionParamsWhenCallIndexMethodThenControllerReturnsIndexView()
        {
            AddProviderParamsToCurrentUser();
            SetProviderConnectionResponse(HttpStatusCode.OK);

            var result = controller.Index();
            Assert.IsInstanceOfType(result.Result, typeof(RedirectToActionResult));
            var actionResult = (RedirectToActionResult)result.Result;
            Assert.AreEqual("ShowMails", actionResult.ActionName);
        }

        [TestMethod]
        public void GivenProviderWhenConnectMethodReturnsOKThenControllerReturnsIndexView()
        {
            AddProviderParamsToCurrentUser();
            SetProviderConnectionResponse(HttpStatusCode.OK);

            var result = controller.Index();
            Assert.IsInstanceOfType(result.Result, typeof(RedirectToActionResult));
            var actionResult = (RedirectToActionResult)result.Result;
            Assert.AreEqual("ShowMails", actionResult.ActionName);
        }

        //[TestMethod]
        public void GivenProviderWhenConnectMethodReturnsExpectationFailedThenControllerReturnsErrorViewWithFailedConnectionMessage()
        {
            AddProviderParamsToCurrentUser();
            SetProviderConnectionResponse(HttpStatusCode.ExpectationFailed);

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
            AddProviderParamsToCurrentUser();
            var mailList = new List<Mail>();
            for (int pageId = 0; pageId < page * 2; pageId++)
            {
                for (int entryId = 0; entryId < pageSize; entryId++)
                {
                    mailList.Add(new Mail()
                    {
                        ID = new MailKit.UniqueId(Convert.ToUInt32(pageId) * Convert.ToUInt32(pageSize) + Convert.ToUInt32(entryId) + 1),
                        Date = DateTime.Now.AddDays(-(pageId * pageSize + entryId))
                    });
                }
            }


            mockProvider.Setup(mock => mock.GetMailsFromFolder(It.Is<string>(folder => folder.Equals("inbox")),
                 It.Is<int>(p => p == page), It.Is<int>(ps => ps == pageSize), It.Is<string>(sortOder => sortOder.Equals("dateAsc"))))
                 .Returns(new Tuple<IList<Mail>, HttpStatusCode>(mailList.Skip(pageSize * (page - 1)).Take(pageSize).ToList(), HttpStatusCode.OK));
            var mockMailFolder = new Mock<IMailFolder>();
            mockMailFolder.Setup(mock => mock.Count).Returns(pageSize);
            mockProvider.Setup(mock => mock.GetFolder(It.Is<string>(name => name.Equals("inbox")))).Returns(mockMailFolder.Object);


            SetProviderConnectionResponse(HttpStatusCode.OK);
            SetProviderAuthenticationState(true);

            var viewResult = controller.ShowMails(page, pageSize, "inbox", "dateAsc", null, null).Result as ViewResult;
            Assert.IsNull(viewResult.ViewName);
            Assert.IsNull(viewResult.ViewData["ErrorMessage"]);

            Assert.IsInstanceOfType(viewResult.Model, typeof(IPagedList<Mail>));
            var receivedModel = (IPagedList<Mail>)viewResult.Model;
            Assert.IsNotNull(receivedModel);
            var receivedMails = (IList<Mail>)(receivedModel.ToList());
            Assert.AreEqual(pageSize, receivedMails.Count);
            for (int entryId = 1; entryId < pageSize; entryId++)
            {
                Assert.AreEqual(Convert.ToUInt32(page - 1) * Convert.ToUInt32(pageSize) + Convert.ToUInt32(entryId) + 1, receivedMails.ElementAt(entryId).ID.Id);
            }

        }

        [TestMethod]
        public void GivenMailMessageWhenDetailsMethodIsCalledThenMailMessageIsSetToRead()
        {
            AddProviderParamsToCurrentUser();
            SetProviderConnectionResponse(HttpStatusCode.OK);

            var mail = new Mail();
            mail.IsRead = false;

            mockProvider.Setup(mock => mock.GetMailFromFolder(It.IsAny<UniqueId>(), It.IsAny<string>())).
            Returns(new Tuple<Mail, HttpStatusCode>(mail, HttpStatusCode.OK));
            mockProvider.Setup(mock => mock.MarkEmailAsRead(It.IsAny<UniqueId>(), It.IsAny<string>())).
            Callback(() => mail.IsRead = true);


            var viewResult = controller.Details(1, "any").Result as ViewResult;
            Assert.IsNull(viewResult.ViewName);

            Assert.IsInstanceOfType(viewResult.Model, typeof(Mail));
            Assert.AreEqual(mail, viewResult.Model);
            Assert.IsTrue(((Mail)viewResult.Model).IsRead);
        }

        private void AddProviderParamsToCurrentUser()
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

        private void AddProviderParamsToOtherUser()
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

        private void SetProviderConnectionResponse(HttpStatusCode statusCode)
        {
            mockProvider.Setup(mock => mock.Connect(It.IsAny<ProviderParams>())).Returns(statusCode);
        }

        private void SetProviderAuthenticationState(bool isAuthenticated)
        {
            mockProvider.Setup(mock => mock.IsAuthenticated()).Returns(isAuthenticated);
        }

        [TestCleanup]
        public void TearDown()
        {
            connection.Close();
        }

        Mock<IUrlHelper> urlHelper;
        Mock<IProvider> mockProvider;
        JanuszMailDbContext mockDbContext;
        MailBoxController controller;
        Mock<FakeUserManager> mockUserManager;
        ApplicationUser fakeUser;

    }
}
