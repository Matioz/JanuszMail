using System;
using System.Net.Mail;
using System.Collections.Generic;
using MailKit;
using MimeKit;
using Microsoft.AspNetCore.Http;

namespace JanuszMail.Models
{
    public class Mail
    {
        public UniqueId ID { get; set; }
        public string Recipient { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Folder { get; set; }
        public bool IsRead { get; set; }
        public DateTime Date { get; set; }
        public MailboxAddress Sender { get; set; }
        public MessageSummary summary { get; set; }
        public MimeMessage mimeMessage { get; set; }
        public List<IFormFile> Attachments { get; set; }
        public List<string> AttachmentFileNames { get; set; }

        public Mail()
        {
            this.Body = "Dummy mail";
        }

        public Mail(MessageSummary Summary, MimeMessage message, string folderName)
        {
            if (message == null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }
            this.Folder = folderName;
            this.summary = Summary;
            this.ID = Summary.UniqueId;
            this.mimeMessage = message;
            this.Subject = message.Subject;
            this.SenderName = message.From[0].Name;
            this.SenderEmail = ((MailboxAddress)message.From[0]).Address;
            this.Date = summary.Date.DateTime;
            this.IsRead = summary.Flags.Value.HasFlag(MessageFlags.Seen);
            this.Body = message.HtmlBody;
            this.Attachments = new List<IFormFile>();
            this.AttachmentFileNames = new List<string>();
            foreach (var attachment in message.Attachments)
            {
                if (!(attachment is MessagePart))
                {
                    var part = (MimePart)attachment;
                    AttachmentFileNames.Add(part.FileName);
                }
            }
        }
        //TODO: Add property for attachments
    }

    public class MailHeader
    {
        public uint ID { get; set; }
        public string Recipient { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }
        public MailHeader(Mail mail)
        {
            this.ID = mail.ID.Id;
            this.Recipient = mail.Recipient;
            this.SenderEmail = mail.SenderEmail;
            this.SenderName = mail.SenderName;
            this.Subject = mail.Subject;
            this.Date = mail.Date;
            this.IsRead = mail.IsRead;
        }
        public static implicit operator MailHeader(Mail mail)
        {
            return new MailHeader(mail);
        }
    }
}