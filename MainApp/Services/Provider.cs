using System;
using System.Collections.Generic;
using System.Net.Mail;
using JanuszMail.Interfaces;
using JanuszMail.Models;
using Microsoft.AspNetCore.Http;

namespace JanuszMail.Services
{
    public class Provider : IProvider
    {
        //TODO: Maciej Plewka should fill these implementations
        public HttpResponse Connect(ProviderParams providerParams)
        {
            throw new NotImplementedException();
        }

        public HttpResponse Disconnect(ProviderParams providerParams)
        {
            throw new NotImplementedException();
        }

        public Tuple<IList<string>, HttpResponse> GetFolders()
        {
            throw new NotImplementedException();
        }

        public Tuple<IList<MailMessage>, HttpResponse> GetMailsFromFolder(string folder, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Tuple<IList<string>, HttpResponse> GetSubjectsFromFolder(string folder, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public bool IsAuthenticated()
        {
            throw new NotImplementedException();
        }

        public HttpResponse MarkEmailAsRead(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }

        public HttpResponse MarkEmailAsUnread(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }

        public HttpResponse MoveEmailToFolder(MailMessage mailMessage, string folder)
        {
            throw new NotImplementedException();
        }

        public HttpResponse RemoveEmail(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }

        public HttpResponse SendEmail(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }
    }
}