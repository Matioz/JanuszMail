using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JanuszMail.Models;

namespace JanuszMail.Controllers
{
    public class WebmailController : Controller
    {
        private readonly JanuszMailContext _context;
        public WebmailController(JanuszMailContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Email.ToListAsync());
        }
    }
}
