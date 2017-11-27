using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using JanuszMail.Models;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit;
using MimeKit;

namespace JanuszMail.Interfaces
{
    public interface IProvider
    {
        bool IsAuthenticated();
        Tuple<Mail, HttpStatusCode> GetMailFromFolder(UniqueId id, string folder);
        Tuple<IList<Mail>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize, string sortOrder);
        Tuple<IList<string>, HttpStatusCode> GetFolders();
        Tuple<IList<string>, HttpStatusCode> GetSubjectsFromFolder(string folder, int page, int pageSize);
        HttpStatusCode SendEmail(MimeMessage mailMessage);
        HttpStatusCode RemoveEmail(UniqueId id, string folder);
        HttpStatusCode MoveEmailToFolder(UniqueId id, string folderSrc, string folderDst);
        HttpStatusCode MarkEmailAsRead(UniqueId id, string folder);
        HttpStatusCode MarkEmailAsUnread(UniqueId id, string folder);
        HttpStatusCode Connect(ProviderParams providerParams);
        HttpStatusCode Disconnect();
        IList<Tuple<string, string>> GetBasicInfo(string folder, int page, int pageSize);
        HttpStatusCode DownloadAttachment(string fileName, UniqueId Id, string folderName);
        IMailFolder GetFolder(string name);
        HttpStatusCode SaveDraft(MimeMessage mailMessage);
    }
}
