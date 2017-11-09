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

            return View();
        }
        public async Task<IActionResult> ShowMails(int? page, int? pageSize, string folder, string sortOrder, string subject, string sender)
        {
            //Should return partial view with PagedList of MailMessages that matches to given params
            //When there is no matching messages then should return partial view with error message
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
            ViewBag.SubjectSortParam = sortOrder == "subjectAsc" ? "subjectDesc" : "subjectAsc";
            ViewBag.SenderSortParam = sortOrder == "senderAsc" ? "senderDesc" : "senderAsc";

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
                    mails = mails.OrderByDescending(mail => mail.Sender);
                    break;
                case "senderAsc":
                    mails = mails.OrderBy(mail => mail.Sender);
                    break;
                default:
                    mails = mails.OrderByDescending(mail => mail.Date);
                    break;
            }

            var results = new StaticPagedList<MimeMessage>(mails, currentPage, currentPageSize, 100);
            if (!results.Any())
            {
                ViewBag.ErrorMessage = "No messages matching criteria";
                return View("Error");
            }
            else
            {
                return View(results);
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            //Should return MailMessage object that timestamp is equals given id

            if (id == null)
            {
                ViewBag.ErrorMessage = "Could not open the message";
                return View();
            }
            var foldersTuple = _provider.GetFolders();
            var folders = foldersTuple.Item1;

            int page = 1;
            int pageSize = 50;
            MimeMessage result;
            foreach (string folder in folders)
            {
                var mailsTuple = _provider.GetMailsFromFolder(folder, page, pageSize);
                var mails = mailsTuple.Item1;
                /*foreach (MimeMessage mail in mails) 
                {
                    if (_provider.getUniqueId() == id)
                    {
                        return View(result);
                    }
                }*/
                ViewBag.ErrorMessage = "Could not open the message";
                return View();
            }

            return View(await _userManager.Users.ToListAsync());
        }

        public async Task<IActionResult> Delete(int? id)
        {
            //Should remove mail from provider's server
            return View(await _userManager.Users.ToListAsync());
        }

        // GET: MailBox/Create
        public IActionResult Send()
        {
            return View();
        }

        // POST: MailBox/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send([Bind("ID,Recipient,Subject,Body")] Mail mail)
        {
            if (ModelState.IsValid)
            {
                //Should send an email with attaching current time as timestamp
                await _userManager.Users.ToListAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mail);
        }

        private readonly IProvider _provider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JanuszMailDbContext _dbContext;
    }
}