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
            try{
                _imapClient.Connect (providerParams.ImapServerName, providerParams.ImapPortNumber, true);
            }
            catch(Exception ex){
                return HttpStatusCode.ExpectationFailed;
            }
            try{
                _smtpClient.Connect (providerParams.SmtpServerName, providerParams.SmtpPortNumber, true);
            }
            catch(Exception ex){
                return HttpStatusCode.ExpectationFailed;
            }

            _imapClient.AuthenticationMechanisms.Remove ("XOAUTH2");

            try{
                _smtpClient.Authenticate (providerParams.EmailAdress, providerParams.Password);
            }
            catch(Exception ex){
                return HttpStatusCode.ExpectationFailed;
            }
            try{
                _imapClient.Authenticate (providerParams.EmailAdress, providerParams.Password);
            }
            catch(Exception ex){
                return HttpStatusCode.ExpectationFailed;
            }
            if(_imapClient.IsConnected && _smtpClient.IsConnected){
                return HttpStatusCode.Created;
            }
            else{
               return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode Disconnect(ProviderParams providerParams)
        {
            if(_imapClient.IsConnected && _smtpClient.IsConnected){
                try{
                    _imapClient.Disconnect(true);
                 }
                catch(Exception ex){
                    return HttpStatusCode.ExpectationFailed;
                }
                try{
                    _smtpClient.Disconnect(true);
                }
                catch(Exception ex){
                    return HttpStatusCode.ExpectationFailed;
                }
                if(_imapClient.IsConnected && _smtpClient.IsConnected){
                    return HttpStatusCode.InternalServerError;
                }
                return HttpStatusCode.Created;
            }
            else{
                return HttpStatusCode.InternalServerError;
            }
        }
        public Tuple<IList<string>, HttpStatusCode> GetFolders()
        {
            IList<string> Folders =new List<string>();
            if(!_imapClient.IsConnected){
                return new Tuple<IList<string>, HttpStatusCode>(Folders, HttpStatusCode.InternalServerError);
            }
            var personal = _imapClient.GetFolder (_imapClient.PersonalNamespaces[0]);
                foreach (var folder in personal.GetSubfolders (false)){
                    foreach (var subfolder in folder.GetSubfolders(false)){
	                    Folders.Add(subfolder.Name);
                    }
                    Folders.Add(folder.Name);
                }
            return new Tuple<IList<string>, HttpStatusCode>(Folders, HttpStatusCode.Created);
        }

        public Tuple<IList<MimeMessage>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize)
        {
            if(!_imapClient.IsConnected){
                return new Tuple<IList<MimeMessage>, HttpStatusCode>(null, HttpStatusCode.InternalServerError);
            }
            IMailFolder mailFolder=getFolder(folder);
            mailFolder.Open (FolderAccess.ReadWrite);
            IList<MimeMessage> Mails = new List<MimeMessage>();
            for (int i = page*pageSize; i < mailFolder.Count && i < pageSize; i++) 
                    {
                        var message = mailFolder.GetMessage (i);
                        Mails.Insert(0, message);
                    }
            mailFolder.Close();
            return new Tuple<IList<MimeMessage>, HttpStatusCode>(Mails, HttpStatusCode.Created);
        }

        public Tuple<IList<string>, HttpStatusCode> GetSubjectsFromFolder(string folder, int page, int pageSize)
        {
            if(!_imapClient.IsConnected){
                return new Tuple<IList<string>, HttpStatusCode>(null, HttpStatusCode.InternalServerError);
            }
            IMailFolder mailFolder=getFolder(folder);
            if(mailFolder == null){
                return new Tuple<IList<string>, HttpStatusCode>(new List<string>(), HttpStatusCode.InternalServerError);
            }
            mailFolder.Open (FolderAccess.ReadWrite);
            IList<string> Subjects = new List<string>();
            for (int i = page*pageSize; i < mailFolder.Count && i < pageSize; i++) 
                    {
                        var message = mailFolder.GetMessage (i);
                        Subjects.Insert(0, message.Subject);
                    }
            mailFolder.Close();
            return new Tuple<IList<string>, HttpStatusCode>(Subjects, HttpStatusCode.Created);
        }

        public bool IsAuthenticated()
        {
            return _imapClient.IsAuthenticated;
        }

        public HttpStatusCode MarkEmailAsRead(MimeMessage mailMessage, string folder)
        {
            IMailFolder mailFolder=getFolder(folder);
            if(folder!=null){
                mailFolder.Open (FolderAccess.ReadWrite);
                mailFolder.AddFlags(getUniqueId(mailMessage, folder), MessageFlags.Seen, true);
                mailFolder.Close();
                return HttpStatusCode.Created;
            }
            else{
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode MarkEmailAsUnread(MimeMessage mailMessage, string folder)
        {
            IMailFolder mailFolder=getFolder(folder);
            if(mailFolder != null){
                mailFolder.Open (FolderAccess.ReadWrite);
                mailFolder.RemoveFlags(getUniqueId(mailMessage, folder), MessageFlags.Seen, true);
                mailFolder.Close();
                return HttpStatusCode.Created;
            }
            else{
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode MoveEmailToFolder(MimeMessage mailMessage, string folderSrc, string folderDst)
        {
            IMailFolder mailFolder=getFolder(folderSrc);
            if(mailFolder != null){
            mailFolder.Open (FolderAccess.ReadWrite);
            mailFolder.MoveTo(getUniqueId(mailMessage, folderSrc), getFolder(folderDst));
            return HttpStatusCode.Created;
            }
            else{
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode RemoveEmail(MimeMessage mailMessage, string folder)
        {
            IMailFolder mailFolder=getFolder(folder);
            if(mailFolder != null){
                mailFolder.Open (FolderAccess.ReadWrite);
                mailFolder.AddFlags(getUniqueId(mailMessage, folder), MessageFlags.Deleted, true);
                mailFolder.Expunge();
                mailFolder.Close();
                return HttpStatusCode.Created;
            }
            else{
                return HttpStatusCode.InternalServerError;
            }
        }

        public HttpStatusCode SendEmail(MimeMessage mailMessage)
        {
            _smtpClient.Send(mailMessage);
            return HttpStatusCode.Created;
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
            bool open = mailFolder.IsOpen;
            if(!open){
                mailFolder.Open (FolderAccess.ReadOnly);
            }
            var items = mailFolder.Fetch (0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);
            foreach (var item in items) {
                var message = mailFolder.GetMessage (item.UniqueId);
                if(message.MessageId == mailMessage.MessageId){
                    id = item.UniqueId;
                    break;
                }
            }if(!open){
                mailFolder.Close ();
            }
            return id;
        }
    }
}