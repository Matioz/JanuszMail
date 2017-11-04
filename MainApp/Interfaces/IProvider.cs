using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using JanuszMail.Models;

namespace JanuszMail.Interfaces
{
    public interface IProvider
    {
        bool IsAuthenticated();
        Tuple<IList<MailMessage>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize);
        Tuple<IList<string>, HttpStatusCode> GetFolders();
        Tuple<IList<string>, HttpStatusCode> GetSubjectsFromFolder(string folder, int page, int pageSize);
        HttpStatusCode SendEmail(MailMessage mailMessage);
        HttpStatusCode RemoveEmail(MailMessage mailMessage);
        HttpStatusCode MoveEmailToFolder(MailMessage mailMessage, string folder);
        HttpStatusCode MarkEmailAsRead(MailMessage mailMessage);
        HttpStatusCode MarkEmailAsUnread(MailMessage mailMessage);
        HttpStatusCode Connect(ProviderParams providerParams);
        HttpStatusCode Disconnect(ProviderParams providerParams);
    }
}
