using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JanuszMail.Interfaces;
using JanuszMail.Data;
using JanuszMail.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;

using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit;
using MimeKit;
using PagedList.Core;
namespace JanuszMail.Controllers
{
    [Authorize]
    public class MailBoxController : Controller
    {
        private ProviderParams _providerParams;
        private MailBoxViewModel _mailBoxViewModel;
        private string currentFolder;

        public MailBoxController(IProvider provider, UserManager<ApplicationUser> userManager, JanuszMailDbContext dbContext)
        {
            this._provider = provider;
            this._userManager = userManager;
            this._dbContext = dbContext;
            this._mailBoxViewModel = new MailBoxViewModel();

        }

        private async Task<bool> ConnectToProvider()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var providerParams = _dbContext.ProviderParams.Where(p => p.UserId == user.Id).ToList();

            if (providerParams.Any())
            {
                _providerParams = providerParams.First();
                var result = _provider.Connect(_providerParams);
                return result == HttpStatusCode.OK;
            }
            return false;
        }

        // GET: MailBox
        public async Task<IActionResult> Index()
        {
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                ViewBag.ErrorMessage = "You have no provider selected. Go to Manage and add provider.";
                return View("Error");
            }

            //TODO add a smart way of determining folder parameter, one that would at least ignore case
            return RedirectToAction(nameof(ShowMails), new {folder="INBOX", sortOrder="dateDesc", subject="", sender=""});
        }
        public async Task<IActionResult> ShowMails(int? page, int? pageSize, string folder, string sortOrder, string subject, string sender)
        {
            //Should return partial view with PagedList of MailMessages that matches to given params
            //When there is no matching messages then should return partial view with error message
            ViewBag.Folder = folder;
            ViewBag.ReturnUrl = Url.Action("ShowMails", new { page = page, pageSize = pageSize, folder = folder, sortOrder = sortOrder, sender = sender, subject = subject });
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                ViewBag.ErrorMessage = "Something wrong with connection";
                return View("Error");
            }
            int currentPage = page ?? 1;
            int currentPageSize = pageSize ?? 25;
            var mailsTuple = await Task.Run(() => { return _provider.GetMailsFromFolder(folder, currentPage, currentPageSize); });
            var httpStatusCode = mailsTuple.Item2;

            if (!httpStatusCode.Equals(HttpStatusCode.OK))
            {
                ViewBag.ErrorMessage = "Downloading messages from server failed";
                return View("Error");
            }

            var mails = mailsTuple.Item1.AsQueryable();

            if (!String.IsNullOrEmpty(subject))
            {
                mails = mails.Where(mail => mail.Subject.Contains(subject));
            }
            if (!String.IsNullOrEmpty(sender))
            {
                mails = mails.Where(mail => mail.Sender.Address.Contains(sender) || mail.Sender.Name.Contains(sender));
            }

            ViewBag.DateSortParam = String.IsNullOrEmpty(sortOrder) ? "dateAsc" : ViewBag.DateSortParam;
            ViewBag.CurrentPageSize = currentPageSize;
            ViewBag.CurrectSortOrder = sortOrder;

            switch (sortOrder)
            {
                case "dateAsc":
                    mails = mails.OrderBy(mail => mail.Date);
                    break;
                default:
                    mails = mails.OrderByDescending(mail => mail.Date);
                    break;
            }
            currentFolder = folder;
            List<Mail> myMails = mails.ToList();
            var results = new StaticPagedList<Mail>(myMails, currentPage, currentPageSize, 100);
            if (!results.Any())
            {
                ViewBag.ErrorMessage = "No messages matching criteria";
                return View("Error");
            }
            else
            {
                //_provider.DownloadAttachment("Duda.jpg", myMails.First().ID, currentFolder);
                return View(results);
            }
        }

        public async Task<IActionResult> Details(int? id, string folder)
        {
            //Should return MailMessage object that timestamp is equals given id
            //TODO: add awaits
            if (id == null)
            {
                ViewBag.ErrorMessage = "Could not open the message";
                return View("Error");
            }
            if (String.IsNullOrEmpty(folder))
            {
                folder = "Inbox";
            }

            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                ViewBag.ErrorMessage = "You have no provider selected. Go to Manage and add provider.";
                return View("Error");
            }

            var tuple = await Task.Run(() => { return _provider.GetMailFromFolder(new UniqueId(Convert.ToUInt32(id)), folder);});
            var mail = tuple.Item1;
            var httpStatusCode = tuple.Item2;

            if (!httpStatusCode.Equals(HttpStatusCode.OK))
            {
                ViewBag.ErrorMessage("Could not open the requested e-mail");
                return View("Error");
            }
            else 
            {
                ViewBag.Folder = folder;
                ViewBag.ReturnUrlFailing = Url.Action("Details", new { subject = mail.Subject, folder = folder });
                ViewBag.ReturnUrlPassing = Url.Action("ShowMails", new { folder = folder });
                return View();
            }
        }
        public async Task<IActionResult> Delete(int? id, string folder, string returnUrlPassing, string returnUrlFailing)
        {
            if (id == null)
            {
                if (folder == null || folder.Length == 0)
                {
                    return Redirect(Url.Action("Index"));
                }
                else
                {
                    ViewBag.ErrorMessage = "Message or value not specified";
                    return Redirect(returnUrlFailing);
                }
            }
            if (String.IsNullOrEmpty(folder))
            {
                folder = "Inbox";
            }
            var statusCode = await Task.Run(() => {return _provider.RemoveEmail(new UniqueId(Convert.ToUInt32(id)), folder);});
            if (statusCode.Equals(HttpStatusCode.OK))
            {
                return Redirect(returnUrlPassing);
            }
            else
            {
                ViewBag.ErrorMessage = "Could not access e-mail server";
                return Redirect(returnUrlFailing);
            }
        }

        // GET: MailBox/Create
        public IActionResult Send(string replyTo, string folder)
        {
            ViewBag.Folder = folder;
            var mail = new Mail();
            mail.SenderEmail = replyTo;
            return View(mail);
        }

        // POST: MailBox/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([Bind("ID,Recipient,Subject,Body")] Mail mail)
        {
            if (ModelState.IsValid)
            {
                HttpStatusCode httpStatusCode = await Task.Run(() => {return _provider.SendEmail(mail.mimeMessage);});
                if (httpStatusCode.Equals(HttpStatusCode.OK))
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.ErrorMessage("Could not send the message");
                    return View(mail);
                }
            }
            return View(mail);
        }
        [HttpGet]
        private async Task<ActionResult> DownloadAttachment(UniqueId id)
        {
            string fileName = "Duda.jpg";
            var code = _provider.DownloadAttachment(fileName, id, currentFolder);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int? id, string folder, string returnUrlPassing, string returnUrlFailing)
        {
            if (id == null)
            {
                if (folder == null || folder.Length == 0)
                    return Redirect(Url.Action("Index"));
                else
                {
                    ViewBag.ErrorMessage = "Message or value not specified";
                    return Redirect(returnUrlFailing);
                }
            }
            var mail = new Mail();
            HttpStatusCode result = await Task.Run(() => { return _provider.MarkEmailAsRead(mail, folder); });
            if (result == HttpStatusCode.OK)
            {
                return Redirect(returnUrlPassing);
            }
            else
            {
                ViewBag.ErrorMessage = "Something wrong with marking message";
                return Redirect(returnUrlFailing);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkUnread(int? id, string folder, string returnUrlPassing, string returnUrlFailing)
        {
            if (id == null)
            {
                if (folder == null || folder.Length == 0)
                    return Redirect(Url.Action("Index"));
                else
                {
                    ViewBag.ErrorMessage = "Message or value not specified";
                    return Redirect(returnUrlFailing);
                }
            }
            var mail = new Mail();
            HttpStatusCode result = await Task.Run(() => { return _provider.MarkEmailAsUnread(mail, folder); });

            if (result == HttpStatusCode.OK)
            {
                return Redirect(returnUrlPassing);
            }
            else
            {
                ViewBag.ErrorMessage = "Something wrong with marking message";
                return Redirect(returnUrlFailing);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToFolder(int? id, string folder, string destFolder, string returnUrlPassing, string returnUrlFailing)
        {
            if (id == null)
            {
                if (folder == null || folder.Length == 0)
                    return Redirect(Url.Action("Index"));
                else
                {
                    ViewBag.ErrorMessage = "Message or value not specified";
                    return Redirect(returnUrlFailing);
                }
            }
            Mail mail = new Mail();
            HttpStatusCode result = await Task.Run(() => { return _provider.MoveEmailToFolder(mail, folder, destFolder); });

            if (result == HttpStatusCode.OK)
            {
                return Redirect(returnUrlPassing);
            }
            else
            {
                ViewBag.ErrorMessage = "Something wrong with marking message";
                return Redirect(returnUrlFailing);
            }
        }
        private readonly IProvider _provider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JanuszMailDbContext _dbContext;
    }
}
