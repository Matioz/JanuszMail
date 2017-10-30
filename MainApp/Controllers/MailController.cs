using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MainApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MainApp.Controllers
{
    public class MailController
    {

        public MailController(IProvider provider)
        {
            this._provider = provider;
        }

        public async Task<IActionResult> Index()
        {
            var provderResponse = _provider.GetFolders();
            throw new NotImplementedException();
        }

        public IList<string> folders;
        private IProvider _provider;
    }
}