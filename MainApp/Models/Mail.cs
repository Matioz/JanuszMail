using System;
using System.Net.Mail;
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

        public Mail(MessageSummary Summary, MimeMessage message)
        {
            if (message == null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }
            this.summary = Summary;
            this.Subject = message.Subject;
            this.SenderName = message.From[0].Name;
            this.Date = message.Date.Date;
            this.IsRead = summary.Flags.Value.HasFlag(MessageFlags.Seen);
        }
        //TODO: Add property for attachments
    }
}