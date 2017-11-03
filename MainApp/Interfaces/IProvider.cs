using System;
using System.Collections.Generic;
using System.Net.Mail;
using JanuszMail.Models;
using Microsoft.AspNetCore.Http;

namespace JanuszMail.Interfaces
{
    public interface IProvider
    {
        bool IsAuthenticated();
        Tuple<IList<MailMessage>, HttpResponse> GetMailsFromFolder(string folder, int page, int pageSize);
        Tuple<IList<string>, HttpResponse> GetFolders();
        Tuple<IList<string>, HttpResponse> GetSubjectsFromFolder(string folder, int page, int pageSize);
        HttpResponse SendEmail(MailMessage mailMessage);
        HttpResponse RemoveEmail(MailMessage mailMessage);
        HttpResponse MoveEmailToFolder(MailMessage mailMessage, string folder);
        HttpResponse MarkEmailAsRead(MailMessage mailMessage);
        HttpResponse MarkEmailAsUnread(MailMessage mailMessage);
        HttpResponse Connect(ProviderParams providerParams);
        HttpResponse Disconnect(ProviderParams providerParams);
    }
}
