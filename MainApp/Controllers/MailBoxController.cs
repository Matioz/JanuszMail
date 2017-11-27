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
using System.IO;
using Microsoft.AspNetCore.Http;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit;
using MimeKit;
using PagedList.Core;
using JanuszMail.Extensions;

namespace JanuszMail.Controllers
{
    [Authorize]
    public class MailBoxController : Controller
    {
        private ProviderParams _providerParams;
        private MailBoxViewModel _mailBoxViewModel;

        public MailBoxController(IProvider provider, UserManager<ApplicationUser> userManager, JanuszMailDbContext dbContext)
        {
            this._provider = provider;
            this._userManager = userManager;
            this._dbContext = dbContext;
            this._mailBoxViewModel = new MailBoxViewModel();

        }

        private async Task<bool> ConnectToProvider()
        {
            System.GC.Collect();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var providerParams = _dbContext.ProviderParams.Where(p => p.UserId == user.Id).ToList();

            if (providerParams.Any())
            {
                _providerParams = providerParams.First();
                HttpStatusCode result;
                result = _provider.Connect(_providerParams);
                return result == HttpStatusCode.OK;
            }
            return false;
        }

        // GET: MailBox
        public async Task<IActionResult> Index(string replyTo = null)
        {
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                TempData["ErrorMessage"] = "Cannot connect to provider, make sure you added correct provider";
                return View("Error");
            }
            return View(model: replyTo);
        }

        public async Task<IActionResult> ShowMails(int? page, int? pageSize, string folder, string sortOrder, string subject, string sender)
        {
            //Should return partial view with PagedList of MailMessages that matches to given params
            //When there is no matching messages then should return partial view with error message
            ViewBag.Folder = folder;
            // var connectionStatus = await ConnectToProvider();
            // if (!connectionStatus)
            // {
            //     TempData["ErrorMessage"] = "Something wrong with connection";
            //     return null;
            // }
            int currentPage = page ?? 1;
            int currentPageSize = pageSize ?? 25;
            // var mailsTuple = await Task.Run(() => { return _provider.GetMailsFromFolder(folder, currentPage, currentPageSize, sortOrder); });
            // var httpStatusCode = mailsTuple.Item2;

            // if (!httpStatusCode.Equals(HttpStatusCode.OK))
            // {
            //     TempData["ErrorMessage"] = "Downloading messages from server failed";
            //     return null;
            // }

            var cachedMails = await GetCachedMails(folder);

            var mails = cachedMails.AsQueryable().Skip(currentPageSize * (currentPage - 1)).Take(currentPageSize);

            switch (sortOrder)
            {
                case "dateAsc":
                    mails = mails.OrderBy(mail => mail.Date);
                    break;
                case "subjectDesc":
                    mails = mails.OrderByDescending(mail => mail.Subject);
                    break;
                case "subjectAsc":
                    mails = mails.OrderBy(mail => mail.Subject);
                    break;
                case "senderDesc":
                    mails = mails.OrderByDescending(mail => mail.SenderEmail);
                    break;
                case "senderAsc":
                    mails = mails.OrderBy(mail => mail.SenderEmail);
                    break;
                default:
                    mails = mails.OrderByDescending(mail => mail.Date);
                    break;
            }
            if (!String.IsNullOrEmpty(subject))
            {
                mails = mails.Where(mail => mail.Subject.Contains(subject));
            }
            if (!String.IsNullOrEmpty(sender))
            {
                mails = mails.Where(mail => mail.SenderEmail.Contains(sender) || mail.SenderName.Contains(sender));
            }

            ViewBag.DateSortParam = String.IsNullOrEmpty(sortOrder) ? "dateAsc" : ViewBag.DateSortParam;
            ViewBag.SubjectSortParam = sortOrder == "subjectAsc" ? "subjectDesc" : "subjectAsc";
            ViewBag.SenderSortParam = sortOrder == "senderAsc" ? "senderDesc" : "senderAsc";
            ViewBag.CurrentPageSize = currentPageSize;
            ViewBag.CurrectSortOrder = sortOrder;

            //List<MailHeader> headers = mails.Select<Mail, MailHeader>(x => x).ToList();
            var results = new StaticPagedList<MailHeader>(mails, currentPage, currentPageSize, cachedMails.Count);
            return PartialView("_ShowMails", results);
        }

        public async Task<IActionResult> Details(uint? id, string folder)
        {
            //Should return MailMessage object that timestamp is equals given id

            if (id == null)
            {
                TempData["ErrorMessage"] = "Could not open the message";
                return PartialView("Error");
            }
            if (String.IsNullOrEmpty(folder))
            {
                folder = "Inbox";
            }
            uint ID = id ?? 0; //otherwise it won't compile

            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                TempData["ErrorMessage"] = "You have no provider selected. Go to Manage and add provider.";
                return PartialView("Error");
            }

            var tuple = await Task.Run(() => { return _provider.GetMailFromFolder(new UniqueId(ID), folder); });
            var markReadTask = Task.Run(() => _provider.MarkEmailAsRead(new UniqueId(ID), folder));
            var mail = tuple.Item1;
            var httpStatusCode = tuple.Item2;
            if (mail.summary.Flags.Value.HasFlag(MessageFlags.Draft))
            {
                return PartialView("_Send", model: mail);
            }

            if (!httpStatusCode.Equals(HttpStatusCode.OK))
            {
                TempData["ErrorMessage"] = "Could not open the requested e-mail";
                return PartialView("Error");
            }
            else
            {
                ViewBag.Folder = folder;
                await markReadTask;
                return PartialView("_Details", model: mail);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> Delete(uint? id, string folder)
        {
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                return false;
            }

            if (id == null || folder == null)
            {
                return false;
            }
            uint ID = id ?? 0; //otherwise it won't compile
            if (String.IsNullOrEmpty(folder))
            {
                folder = "Inbox";
            }
            var statusCode = await Task.Run(() => { return _provider.RemoveEmail(new UniqueId(ID), folder); });
            return statusCode.Equals(HttpStatusCode.OK);
        }

        // GET: MailBox/Create
        public async Task<IActionResult> Send(string replyTo)
        {
            var getSignatureTask = Task.Run(async () =>
            {
                var user = await _userManager.GetUserAsync(User);
                if (user.Signature != null && user.Signature.Length != 0)
                {
                    return "<br/><br/><br/>" + user.Signature;
                }
                return "";
            });
            var mail = new Mail();
            if (!String.IsNullOrEmpty(replyTo))
            {
                mail.Recipient = replyTo;
            }
            mail.Body = await getSignatureTask;
            return PartialView("_Send", mail);
        }

        private async Task<MimeMessage> ConstructMimeMessage(Mail mail)
        {
            var builder = new BodyBuilder();
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_providerParams.EmailAddress));
            mimeMessage.To.Add(new MailboxAddress(mail.Recipient));
            mimeMessage.Subject = mail.Subject;
            if (mail.Attachments != null)
                foreach (var attachment in mail.Attachments)
                {
                    var tempPath = Path.GetTempPath();
                    if (attachment.Length > 0)
                    {
                        var filePath = Path.Combine(tempPath, attachment.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await attachment.CopyToAsync(stream);
                        }
                        builder.Attachments.Add(filePath);
                        System.IO.File.Delete(filePath);
                    }
                }
            builder.HtmlBody = mail.Body;
            mimeMessage.Body = builder.ToMessageBody();
            return mimeMessage;
        }

        // POST: MailBox/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> Send([Bind("ID,Recipient,Subject,Attachments,Body")] Mail mail)
        {
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                return false;
            }
            if (ModelState.IsValid)
            {
                mail.mimeMessage = await ConstructMimeMessage(mail);

                HttpStatusCode httpStatusCode = await Task.Run(() => { return _provider.SendEmail(mail.mimeMessage); });
                if (httpStatusCode.Equals(HttpStatusCode.OK))
                {
                    return true;
                }
            }
            return false;
        }
        [HttpGet, ActionName("DownloadAttachment")]
        public async Task<ActionResult> DownloadAttachment(int id, string fileName, string folder)
        {
            var connectionStatus = await ConnectToProvider();
            var code = _provider.DownloadAttachment(fileName, new UniqueId((uint)id), folder);
            if (!System.IO.File.Exists(fileName))
            {
                //return HttpStatusCode.ExpectationFailed;
            }
            var fileBytes = System.IO.File.ReadAllBytes(fileName);
            var response = new FileContentResult(fileBytes, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
            System.IO.File.Delete(fileName);
            return response;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> MarkRead(uint? id, string folder)
        {
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                return false;
            }

            if (id == null || folder == null)
            {
                return false;
            }
            HttpStatusCode result = await Task.Run(() => { return _provider.MarkEmailAsRead(new UniqueId(id ?? 0), folder); });
            return (result == HttpStatusCode.OK);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> MarkUnread(uint? id, string folder)
        {
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                return false;
            }

            if (id == null || folder == null)
            {
                return false;
            }
            HttpStatusCode result = await Task.Run(() => { return _provider.MarkEmailAsUnread(new UniqueId(id ?? 0), folder); });

            return (result == HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<bool> MoveToFolder(uint? id, string folder, string destFolder)
        {
            var connectionStatus = await ConnectToProvider();
            if (!connectionStatus)
            {
                return false;
            }

            if (id == null || folder == null)
            {
                return false;
            }
            HttpStatusCode result = await Task.Run(() => { return _provider.MoveEmailToFolder(new UniqueId(id ?? 0), folder, destFolder); });

            return (result == HttpStatusCode.OK);
        }

        public async Task<IActionResult> QuickSearch(string folder, string subject, string sender)
        {
            if (folder == null)
            {
                return Json(null);
            }
            var mailList = await GetCachedMails(folder);
            var mails = mailList.AsQueryable();
            if (subject != null)
            {
                mails = mails.Where(mail => mail.Subject.Contains(subject));
                return Json(mails.Select(mail => mail.Subject).Distinct().Take(5).ToList());
            }
            if (sender != null)
            {
                mails = mails.Where(mail => mail.SenderName.Contains(sender) || mail.SenderEmail.Contains(sender));
                return Json(mails.Select(mail => mail.SenderEmail).Distinct().Take(5).ToList());
            }
            return Json(null);
        }

        private async Task<IList<MailHeader>> GetCachedMails(string folder)
        {

            var currentList = HttpContext.Session.GetObjectFromJson<IList<MailHeader>>(folder);
            if (currentList == null)
            {
                await UpdateCachedMails(folder);
                return HttpContext.Session.GetObjectFromJson<IList<MailHeader>>(folder);
            }
            return currentList;
        }
        public async Task<bool> UpdateCachedMails(string folder)
        {
            var connectionStatus = await ConnectToProvider();

            if (!connectionStatus)
            {
                return false;
            }
            var mailsTuple = await Task.Run(() => { return _provider.GetMailsFromFolder(folder, 1, 100, "dateDesc"); });
            var httpStatusCode = mailsTuple.Item2;
            if (httpStatusCode == HttpStatusCode.OK)
            {
                var fullMailList = mailsTuple.Item1;
                var latestList = fullMailList.Select<Mail, MailHeader>(x => x).ToList();
                HttpContext.Session.SetObjectAsJson(folder, latestList);
                return true;
            }
            return false;
        }

        public async Task<bool> SaveDraft([Bind("ID,Recipient,Subject,Body")] Mail mail)
        {
            var connectionStatus = await ConnectToProvider();
            if (mail.Recipient == null)
            {
                mail.Recipient = "";
            }
            if (mail.Subject == null)
            {
                mail.Subject = "";
            }
            if (mail.Body == null)
            {
                mail.Body = "";
            }

            if (!connectionStatus)
            {
                return false;
            }

            if (ModelState.IsValid)
            {
                mail.mimeMessage = await ConstructMimeMessage(mail);

                HttpStatusCode httpStatusCode = await Task.Run(() => { return _provider.SaveDraft(mail.mimeMessage); });
                if (httpStatusCode.Equals(HttpStatusCode.OK))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        private readonly IProvider _provider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JanuszMailDbContext _dbContext;
    }
}
