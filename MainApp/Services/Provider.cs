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
        private ImapClient _imapClient;
        private MailKit.Net.Smtp.SmtpClient _smtpClient;
        ~Provider()
        {
            Disconnect();
        }
        public HttpStatusCode Connect(ProviderParams providerParams)
        {
            _imapClient = new ImapClient();
            _smtpClient = new MailKit.Net.Smtp.SmtpClient();
            _imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
            try
            {
                _imapClient.Connect(providerParams.ImapServerName, providerParams.ImapPortNumber, true);
                _smtpClient.Connect(providerParams.SmtpServerName, providerParams.SmtpPortNumber, true);
                _imapClient.AuthenticationMechanisms.Remove("XOAUTH2");
                _smtpClient.Authenticate(providerParams.EmailAddress, providerParams.Password);
                _imapClient.Authenticate(providerParams.EmailAddress, providerParams.Password);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return HttpStatusCode.ExpectationFailed;
            }

            if (_imapClient.IsConnected && _smtpClient.IsConnected)
            {
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode Disconnect()
        {
            if (IsConnected())
            {
                try
                {
                    _imapClient.Disconnect(true);
                    _smtpClient.Disconnect(true);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                    return HttpStatusCode.ExpectationFailed;
                }
                if (_imapClient.IsConnected && _smtpClient.IsConnected)
                {
                    return HttpStatusCode.InternalServerError;
                }
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }
        public Tuple<IList<string>, HttpStatusCode> GetFolders()
        {
            IList<string> Folders = new List<string>();
            if (!IsAuthenticated())
            {
                return new Tuple<IList<string>, HttpStatusCode>(Folders, HttpStatusCode.ExpectationFailed);
            }
            var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);
            foreach (var folder in personal.GetSubfolders(false))
            {
                foreach (var subfolder in folder.GetSubfolders(false))
                {
                    Folders.Add(subfolder.Name);
                }
                Folders.Add(folder.Name);
            }
            return new Tuple<IList<string>, HttpStatusCode>(Folders, HttpStatusCode.OK);
        }

        public Tuple<IList<Mail>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize)
        {
            if (!IsAuthenticated())
            {
                return new Tuple<IList<Mail>, HttpStatusCode>(null, HttpStatusCode.ExpectationFailed);
            }
            IMailFolder mailFolder = GetFolder(folder);
            mailFolder.Open(FolderAccess.ReadWrite);
            List<MessageSummary> Items = mailFolder.Fetch((page - 1) * pageSize, pageSize, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags | MessageSummaryItems.All) as List<MessageSummary>;
            List<Mail> Mails = new List<Mail>();
            if(Items == null){
                Items = new List<MessageSummary>();
            }
            foreach(MessageSummary mail in Items){
                Mails.Add(new Mail(mail, mailFolder.GetMessage(mail.UniqueId)));
            }

            mailFolder.Close();
            return new Tuple<IList<Mail>, HttpStatusCode>(Mails, HttpStatusCode.OK);
        }

        public Tuple<IList<string>, HttpStatusCode> GetSubjectsFromFolder(string folder, int page, int pageSize)
        {
            if (!IsAuthenticated())
            {
                return new Tuple<IList<string>, HttpStatusCode>(null, HttpStatusCode.ExpectationFailed);
            }
            IMailFolder mailFolder = GetFolder(folder);
            if (mailFolder == null)
            {
                return new Tuple<IList<string>, HttpStatusCode>(new List<string>(), HttpStatusCode.InternalServerError);
            }
            mailFolder.Open(FolderAccess.ReadWrite);
            IList<string> Subjects = new List<string>();
            for (int i = page * pageSize; i < mailFolder.Count && i < pageSize; i++)
            {
                var message = mailFolder.GetMessage(i);
                Subjects.Insert(0, message.Subject);
            }
            mailFolder.Close();
            return new Tuple<IList<string>, HttpStatusCode>(Subjects, HttpStatusCode.OK);
        }

        public bool IsAuthenticated()
        {

            return IsConnected() && _imapClient.IsAuthenticated && _smtpClient.IsAuthenticated;
        }

        public HttpStatusCode MarkEmailAsRead(MimeMessage mailMessage, string folder)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folder);
            if (folder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.AddFlags(GetUniqueId(mailMessage, folder), MessageFlags.Seen, true);
                mailFolder.Close();
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode MarkEmailAsUnread(MimeMessage mailMessage, string folder)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folder);
            if (mailFolder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.RemoveFlags(GetUniqueId(mailMessage, folder), MessageFlags.Seen, true);
                mailFolder.Close();
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode MoveEmailToFolder(MimeMessage mailMessage, string folderSrc, string folderDst)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folderSrc);
            if (mailFolder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.MoveTo(GetUniqueId(mailMessage, folderSrc), GetFolder(folderDst));
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode RemoveEmail(MimeMessage mailMessage, string folder)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folder);
            if (mailFolder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.AddFlags(GetUniqueId(mailMessage, folder), MessageFlags.Deleted, true);
                mailFolder.Expunge();
                mailFolder.Close();
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode SendEmail(MimeMessage mailMessage)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            _smtpClient.Send(mailMessage);
            return HttpStatusCode.OK;
        }
        private IMailFolder GetFolder(string name)
        {
            IMailFolder mailFolder = null;
            var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);
            foreach (var folder in personal.GetSubfolders(false))
            {
                foreach (var subfolder in folder.GetSubfolders(false))
                {
                    if (subfolder.Name == name)
                    {
                        mailFolder = subfolder;
                    }
                }
                if (folder.Name == name)
                {
                    mailFolder = folder;
                }
            }
            return mailFolder;
        }

        private UniqueId GetUniqueId(MimeMessage mailMessage, string folder)
        {
            UniqueId id = new UniqueId();
            IMailFolder mailFolder = GetFolder(folder);
            bool open = mailFolder.IsOpen;
            if (!open)
            {
                mailFolder.Open(FolderAccess.ReadOnly);
            }
            var items = mailFolder.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);
            foreach (var item in items)
            {
                var message = mailFolder.GetMessage(item.UniqueId);
                if (message.MessageId == mailMessage.MessageId)
                {
                    id = item.UniqueId;
                    break;
                }
            }
            if (!open)
            {
                mailFolder.Close();
            }
            return id;
        }

        private bool IsConnected()
        {

            return _imapClient != null && _imapClient.IsConnected && _smtpClient != null && _smtpClient.IsConnected;
        }
    }
}