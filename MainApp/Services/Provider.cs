using System;
using System.Collections.Generic;
using System.Net.Mail;
using JanuszMail.Interfaces;
using JanuszMail.Models;
using System.Net;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit;
using MimeKit;

namespace JanuszMail.Services
{
    public class Provider : IProvider
    {
        //TODO: Maciej Plewka should fill these implementations
        private ImapClient _imapClient {get;set;}
        private MailKit.Net.Smtp.SmtpClient _smtpClient {get;set;}
        public List<string> Subjects;
        public HttpStatusCode Connect(ProviderParams providerParams)
        {
            _imapClient = new ImapClient();
            _imapClient.ServerCertificateValidationCallback = (s,c,h,e) => true;
            _imapClient.Connect (providerParams.ImapServerName, providerParams.ImapPortNumber, true);
            _imapClient.AuthenticationMechanisms.Remove ("XOAUTH2");
            _imapClient.Authenticate (providerParams.EmailAdress, providerParams.Password);
            if(_imapClient.IsConnected){
            Subjects = new List<string>();
                return HttpStatusCode.Continue;
            }
            else{
                return HttpStatusCode.NoContent;
            }
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
            if(!_imapClient.IsConnected){
                return new Tuple<IList<string>, HttpStatusCode>(null, HttpStatusCode.NoContent);
            }
            var inbox = _imapClient.Inbox;
            inbox.Open (FolderAccess.ReadOnly);
            Subjects = new List<string>();
            for (int i = page*pageSize; i < inbox.Count && i < pageSize; i++) 
                    {
                        var message = inbox.GetMessage (i);
                        Subjects.Insert(0, message.Subject);
                    }
            return new Tuple<IList<string>, HttpStatusCode>(null, HttpStatusCode.NoContent);
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