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
        Tuple<IList<Mail>, HttpStatusCode> GetMailsFromFolder(string folder, int page, int pageSize);
        Tuple<IList<string>, HttpStatusCode> GetFolders();
        Tuple<IList<string>, HttpStatusCode> GetSubjectsFromFolder(string folder, int page, int pageSize);
        HttpStatusCode SendEmail(MimeMessage mailMessage);
        HttpStatusCode RemoveEmail(Mail mailMessage, string folder);
        HttpStatusCode MoveEmailToFolder(Mail mailMessage, string folderSrc, string folderDst);
        HttpStatusCode MarkEmailAsRead(Mail mailMessage, string folder);
        HttpStatusCode MarkEmailAsUnread(Mail mailMessage, string folder);
        HttpStatusCode Connect(ProviderParams providerParams);
        HttpStatusCode Disconnect();
        IList<Tuple<string, string>> GetBasicInfo(string folder, int page, int pageSize);
        HttpStatusCode DownloadAttachment(string fileName, UniqueId Id, string folderName);
    }
}
