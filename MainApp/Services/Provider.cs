using System;
using System.IO;
using System.Linq;
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
        private string _currentFolder;
        private IEnumerable<IMessageSummary> _currentFolderSummary;
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
                _currentFolder = "";
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

        public Tuple<Mail, HttpStatusCode> GetMailFromFolder(UniqueId id, string folder)
        {
            if (!IsAuthenticated())
            {
                return new Tuple<Mail, HttpStatusCode>(null, HttpStatusCode.ExpectationFailed);
            }
            IMailFolder mailFolder = GetFolder(folder);
            mailFolder.Open(FolderAccess.ReadWrite);
            var Items = mailFolder.Fetch(new List<UniqueId>() { id }, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags | MessageSummaryItems.All);
            if (Items == null)
            {
                Items = new List<IMessageSummary>();
            }
            Mail mail = new Mail((MessageSummary)Items.First(), mailFolder.GetMessage(Items.First().UniqueId), folder);

            mailFolder.Close();
            return new Tuple<Mail, HttpStatusCode>(mail, HttpStatusCode.OK);
        }

        public Tuple<IList<Mail>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize, string sortOrder)
        {
            if (!IsAuthenticated())
            {
                return new Tuple<IList<Mail>, HttpStatusCode>(null, HttpStatusCode.ExpectationFailed);
            }
            if (folder != _currentFolder)
            {
                reloadMessages(folder);
            }
            List<Mail> Mails = new List<Mail>();
            if (_currentFolderSummary == null)
            {
                _currentFolderSummary = new List<IMessageSummary>();
            }
            switch (sortOrder)
            {
                case "dateAsc":
                    _currentFolderSummary = _currentFolderSummary.OrderBy(mail => mail.Date.DateTime);
                    break;
                case "subjectDesc":
                    _currentFolderSummary = _currentFolderSummary.OrderByDescending(mail => mail.NormalizedSubject);
                    break;
                case "subjectAsc":
                    _currentFolderSummary = _currentFolderSummary.OrderBy(mail => mail.NormalizedSubject);
                    break;
                case "senderDesc":
                    _currentFolderSummary = _currentFolderSummary.OrderByDescending(mail => mail.Envelope.From[0]);
                    break;
                case "senderAsc":
                    _currentFolderSummary = _currentFolderSummary.OrderBy(mail => mail.Envelope.From[0]);
                    break;
                default:
                    _currentFolderSummary = _currentFolderSummary.OrderByDescending(mail => mail.Date.DateTime);
                    break;
            }
            IMailFolder mailFolder = GetFolder(folder);
            mailFolder.Open(FolderAccess.ReadWrite);
            foreach (var mail in _currentFolderSummary.Skip((page - 1) * pageSize).Take(pageSize))
            {
                Mails.Add(new Mail((MessageSummary)mail, mailFolder.GetMessage(mail.UniqueId), folder));
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

        public HttpStatusCode MarkEmailAsRead(Mail mailMessage, string folder)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folder);
            if (folder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.AddFlags(mailMessage.ID, MessageFlags.Seen, true);
                mailFolder.Close();
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode MarkEmailAsUnread(Mail mailMessage, string folder)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folder);
            if (mailFolder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.RemoveFlags(mailMessage.ID, MessageFlags.Seen, true);
                mailFolder.Close();
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode MoveEmailToFolder(Mail mailMessage, string folderSrc, string folderDst)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folderSrc);
            if (mailFolder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.MoveTo(mailMessage.ID, GetFolder(folderDst));
                return HttpStatusCode.OK;
            }
            else
            {
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode RemoveEmail(UniqueId id, string folder)
        {
            if (!IsAuthenticated())
            {
                return HttpStatusCode.ExpectationFailed;
            }
            IMailFolder mailFolder = GetFolder(folder);
            if (mailFolder != null)
            {
                mailFolder.Open(FolderAccess.ReadWrite);
                mailFolder.AddFlags(id, MessageFlags.Deleted, true);
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
        public IMailFolder GetFolder(string name)
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
        public IList<Tuple<string, string>> GetBasicInfo(string folder, int page, int pageSize)
        {
            List<Tuple<string, string>> Info = new List<Tuple<string, string>>();
            if (!IsAuthenticated())
            {
                return Info;
            }
            IMailFolder mailFolder = GetFolder(folder);
            mailFolder.Open(FolderAccess.ReadWrite);
            var Items = mailFolder.Fetch((page - 1) * pageSize, pageSize, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags | MessageSummaryItems.All);
            if (Items == null)
            {
                Items = new List<IMessageSummary>();
            }
            foreach (var mail in (Items as List<MessageSummary>))
            {
                Info.Add(new Tuple<string, string>(mail.NormalizedSubject, mail.Envelope.From[0].Name));
            }

            mailFolder.Close();
            return Info;
        }
        public HttpStatusCode DownloadAttachment(string fileName, UniqueId Id, string folderName)
        {
            var folder = GetFolder(folderName);
            folder.Open(FolderAccess.ReadWrite);
            var message = folder.GetMessage(Id);
            var attachment = message.Attachments.Where(x => x.ContentDisposition?.FileName == fileName).ToList().First();
            //It has to be repleced with Dir chhoser
            using (var stream = File.Create(fileName))
            {
                var part = (MimePart)attachment;
                part.ContentObject.DecodeTo(stream);
            }
            folder.Close();
            return HttpStatusCode.OK;
        }
        public void reloadMessages(string folder)
        {
            IMailFolder mailFolder = GetFolder(folder);
            mailFolder.Open(FolderAccess.ReadWrite);
            _currentFolderSummary = mailFolder.Fetch(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags | MessageSummaryItems.Envelope);
            mailFolder.Close();
        }
    }
}