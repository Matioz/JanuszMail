using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JanuszMail.Interfaces;
using JanuszMail.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace JanuszMail.Controllers
{
    [Authorize]
    public class MailBoxController : Controller
    {
        public MailBoxController(IProvider provider, UserManager<ApplicationUser> userManager)
        {
            this._provider = provider;
            this._userManager = userManager;
        }
        // GET: MailBox
        public IActionResult Index()
        {
            //Should connect to provider if it is not
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
    }
}