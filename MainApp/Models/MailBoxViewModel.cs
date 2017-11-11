using System.Collections.Generic;

namespace JanuszMail.Models
{
    public class MailBoxViewModel
    {
        public IList<Mail> Mails;
        public IList<string> Folders;
    }
}