using System.Net;
using System.Threading.Tasks;
using JanuszMail.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JanuszMail.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IProvider _provider;
        public SidebarViewComponent(IProvider provider)
        {
            this._provider = provider;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (await Task.Run(() => { return _provider.IsAuthenticated(); }))
            {
                var folderList = await Task.Run(() => { return _provider.GetFolders(); });
                if (folderList.Item2 == HttpStatusCode.OK)
                {
                    return View(folderList.Item1);
                }
            }
            return View();
        }
    }
}