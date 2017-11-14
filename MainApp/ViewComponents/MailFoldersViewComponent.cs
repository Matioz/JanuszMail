using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using JanuszMail.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JanuszMail.ViewComponents
{
    public class MailFoldersViewComponent : ViewComponent
    {
        private readonly IProvider _provider;
        public MailFoldersViewComponent(IProvider provider)
        {
            this._provider = provider;
        }
        public async Task<IViewComponentResult> InvokeAsync(string viewName)
        {
            if (await Task.Run(() => { return _provider.IsAuthenticated(); }))
            {
                var folderList = await Task.Run(() => { return _provider.GetFolders(); });
                if (folderList.Item2 == HttpStatusCode.OK)
                {
                    return View(viewName, folderList.Item1);
                }
            }
            return View(viewName, new List<string>());
        }
    }
}
