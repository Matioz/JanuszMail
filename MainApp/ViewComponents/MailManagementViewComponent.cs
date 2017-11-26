using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JanuszMail.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JanuszMail.ViewComponents
{
    public class MailManagementViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(bool isMailRead, uint mailId, string folder)
        {
            ViewData["envelopeClass"] = isMailRead == true ? "fa-envelope-open" : "fa-envelope";
            ViewData["markAction"] = isMailRead == true ? "MarkUnread" : "MarkRead";
            ViewData["buttonClass"] = isMailRead == true ? "btn-info" : "btn-warning";
            ViewData["folder"] = folder;
            ViewData["mailId"] = mailId;
            return View();
        }
    }
}
