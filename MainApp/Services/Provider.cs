using System;
using System.Collections.Generic;
using System.Net.Mail;
using JanuszMail.Interfaces;
using JanuszMail.Models;
using System.Net;

namespace JanuszMail.Services
{
    public class Provider : IProvider
    {
        //TODO: Maciej Plewka should fill these implementations
        public HttpStatusCode Connect(ProviderParams providerParams)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode Disconnect(ProviderParams providerParams)
        {
            throw new NotImplementedException();
        }

        public Tuple<IList<string>, HttpStatusCode> GetFolders()
        {
            throw new NotImplementedException();
        }

        public Tuple<IList<MailMessage>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Tuple<IList<string>, HttpStatusCode> GetSubjectsFromFolder(string folder, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public bool IsAuthenticated()
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode MarkEmailAsRead(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode MarkEmailAsUnread(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode MoveEmailToFolder(MailMessage mailMessage, string folder)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode RemoveEmail(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode SendEmail(MailMessage mailMessage)
        {
            throw new NotImplementedException();
        }
    }
}