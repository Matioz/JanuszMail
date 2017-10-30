using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace JanuszMail.Controllers
{
    public class HelloController : Controller
    {
        public IActionResult Index(int ID = 1)
        {
            ViewData["ID"] = ID;
            return View();
        }

        public IActionResult Welcome(string name, int numTimes = 1)
        {
            ViewData["Name"] = name;
            ViewData["NumTimes"] = numTimes;
            return View();
        }
    }
}