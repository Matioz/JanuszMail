using MainApp.Controllers;
using UnitTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Controllers
{
    [TestClass]
    public class MailControllerTest
    {
        [TestMethod]
        public void GivenNotExistingServerWhenMailControllerSynchronizeFoldersThenReturnsMessageWithError404()
        {
            MockProvider provider = new MockProvider { ErrorCode = 404 };
            MailController mailController = new MailController(provider);

            var results = mailController.Index().Result as ViewResult;

            Assert.Equals(404, results.StatusCode);
            Assert.IsNull(results.Model);
        }
    }
}
