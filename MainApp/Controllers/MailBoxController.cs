using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JanuszMail.Interfaces;
using JanuszMail.Data;
using JanuszMail.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;

namespace JanuszMail.Controllers
{
    [Authorize]
    public class MailBoxController : Controller
    {
        public MailBoxController(IProvider provider, UserManager<ApplicationUser> userManager, JanuszMailDbContext dbContext)
        {
            this._provider = provider;
            this._userManager = userManager;
            this._dbContext = dbContext;
        }
        // GET: MailBox
        public async Task<IActionResult> Index()
        {
            //Should connect to provider if it is not


            //Example of getting provider params
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var providerParams = _dbContext.ProviderParams.SingleOrDefaultAsync(p => p.UserId == user.Id);
            return View();
        }
        public async Task<IActionResult> ShowMails(int? page, int? pageSize, string folder, string sortOrder, string subject, string sender)
        {
            //Should return partial view with PagedList of MailMessages that matches to given params
            //When there is no matching messages then should return partial view with error message
            return PartialView("_Search", await _userManager.Users.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            //Should return MailMessage object that timestamp is equals given id
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