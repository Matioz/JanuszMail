using System;
using System.Net.Mail;
using System.Collections.Generic;
using MailKit;
using MimeKit;

namespace JanuszMail.Models
{
    public class Mail
    {
        public UniqueId ID { get; set; }
        public string Recipient { get; set; }
        public string SenderName { get; set; }
        public MailAddress SenderEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Folder { get; set; }
        public bool IsRead { get; set; }
        public DateTime Date { get; set; }
        public MailboxAddress Sender { get; set; }
        public MessageSummary summary { get; set; }
        public MimeMessage mimeMessage { get; set; }
        public List<string> Attachments { get; set; }

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
            this.Date = summary.Date.DateTime;
            this.IsRead = summary.Flags.Value.HasFlag(MessageFlags.Seen);
            this.Body=message.HtmlBody;
            this.Attachments=new List<string>();
            foreach (var attachment in message.Attachments) {
                if (!(attachment is MessagePart)) {
                    var part = (MimePart) attachment;
                    Attachments.Add(part.FileName);
                }
            }
        }
        //TODO: Add property for attachments
    }
}