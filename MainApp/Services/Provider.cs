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
        public HttpStatusCode Connect(ProviderParams providerParams)
        {
            _imapClient = new ImapClient();
            _smtpClient = new MailKit.Net.Smtp.SmtpClient();
            _imapClient.ServerCertificateValidationCallback = (s,c,h,e) => true;
            _imapClient.Connect (providerParams.ImapServerName, providerParams.ImapPortNumber, true);
            _smtpClient.Connect (providerParams.SmtpServerName, providerParams.SmtpPortNumber, true);
            _imapClient.AuthenticationMechanisms.Remove ("XOAUTH2");
            _imapClient.Authenticate (providerParams.EmailAdress, providerParams.Password);
            _smtpClient.Authenticate (providerParams.EmailAdress, providerParams.Password);
            if(_imapClient.IsConnected && _smtpClient.IsConnected){
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
            if(!_imapClient.IsConnected){
                return new Tuple<IList<string>, HttpStatusCode>(null, HttpStatusCode.NoContent);
            }
            IList<string> Folders =new List<string>();
            var personal = _imapClient.GetFolder (_imapClient.PersonalNamespaces[0]);
                foreach (var folder in personal.GetSubfolders (false)){
                    foreach (var subfolder in folder.GetSubfolders(false)){
	                    Folders.Add(subfolder.Name);
                    }
                    Folders.Add(folder.Name);
                }
            return new Tuple<IList<string>, HttpStatusCode>(Folders, HttpStatusCode.Continue);
        }

        public Tuple<IList<MimeMessage>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize)
        {
            if(!_imapClient.IsConnected){
                return new Tuple<IList<MimeMessage>, HttpStatusCode>(null, HttpStatusCode.NoContent);
            }
            IMailFolder mailFolder=getFolder(folder);
            mailFolder.Open (FolderAccess.ReadOnly);
            IList<MimeMessage> Mails = new List<MimeMessage>();
            for (int i = page*pageSize; i < mailFolder.Count && i < pageSize; i++) 
                    {
                        var message = mailFolder.GetMessage (i);
                        Mails.Insert(0, message);
                    }
            mailFolder.Close();
            return new Tuple<IList<MimeMessage>, HttpStatusCode>(Mails, HttpStatusCode.NoContent);
        }

        public Tuple<IList<string>, HttpStatusCode> GetSubjectsFromFolder(string folder, int page, int pageSize)
        {
            if(!_imapClient.IsConnected){
                return new Tuple<IList<string>, HttpStatusCode>(null, HttpStatusCode.NoContent);
            }
            IMailFolder mailFolder=getFolder(folder);
            if(mailFolder == null){
                return new Tuple<IList<string>, HttpStatusCode>(new List<string>(), HttpStatusCode.NoContent);
            }
            mailFolder.Open (FolderAccess.ReadOnly);
            IList<string> Subjects = new List<string>();
            for (int i = page*pageSize; i < mailFolder.Count && i < pageSize; i++) 
                    {
                        var message = mailFolder.GetMessage (i);
                        Subjects.Insert(0, message.Subject);
                    }
            mailFolder.Close();
            return new Tuple<IList<string>, HttpStatusCode>(Subjects, HttpStatusCode.NoContent);
        }

        public bool IsAuthenticated()
        {
            return _imapClient.IsConnected;
        }

        public HttpStatusCode MarkEmailAsRead(MimeMessage mailMessage, string folder)
        {
            IMailFolder mailFolder=getFolder(folder);
            mailFolder.Open (FolderAccess.ReadOnly);
            mailFolder.AddFlags(getUniqueId(mailMessage, folder), MessageFlags.Seen, true);
            mailFolder.Close();
            throw new NotImplementedException();
        }

        public HttpStatusCode MarkEmailAsUnread(MimeMessage mailMessage, string folder)
        {
            IMailFolder mailFolder=getFolder(folder);
            mailFolder.Open (FolderAccess.ReadOnly);
            mailFolder.RemoveFlags(getUniqueId(mailMessage, folder), MessageFlags.Seen, true);
            mailFolder.Close();
            throw new NotImplementedException();
        }

        public HttpStatusCode MoveEmailToFolder(MimeMessage mailMessage, string folderSrc, string folderDst)
        {
            IMailFolder mailFolder=getFolder(folderSrc);
            mailFolder.Open (FolderAccess.ReadOnly);
            mailFolder.MoveTo(getUniqueId(mailMessage, folderSrc), getFolder(folderDst));
            throw new NotImplementedException();
        }

        public HttpStatusCode RemoveEmail(MimeMessage mailMessage, string folder)
        {
            IMailFolder mailFolder=getFolder(folder);
            mailFolder.Open (FolderAccess.ReadOnly);
            mailFolder.AddFlags(getUniqueId(mailMessage, folder), MessageFlags.Deleted, true);
            mailFolder.Close();
            throw new NotImplementedException();
        }

        public HttpStatusCode SendEmail(MimeMessage mailMessage)
        {
            _smtpClient.Send(mailMessage);
            throw new NotImplementedException();
        }
        public IMailFolder getFolder(string name){
            IMailFolder mailFolder=null;
            var personal = _imapClient.GetFolder (_imapClient.PersonalNamespaces[0]);
                foreach (var folder in personal.GetSubfolders (false)){
                    foreach (var subfolder in folder.GetSubfolders(false)){
	                    if(subfolder.Name == name){
                            mailFolder = subfolder;
                        }
                    }
                    if(folder.Name == name){
                            mailFolder = folder;
                        }
                }
            return mailFolder;
        }
        public UniqueId getUniqueId(MimeMessage mailMessage, string folder){
            UniqueId id= new UniqueId();
            IMailFolder mailFolder=getFolder(folder);
            mailFolder.Open (FolderAccess.ReadOnly);
            var items = mailFolder.Fetch (0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);
            foreach (var item in items) {
                var message = mailFolder.GetMessage (item.UniqueId);
                if(message.MessageId == mailMessage.MessageId){
                    mailFolder.Close();
                    id = item.UniqueId;
                }
            }  
            return id;
        }
    }
}