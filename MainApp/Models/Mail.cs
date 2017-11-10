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
        public bool IsRead { get; set; }
        public DateTime Date { get; set; }

        public static explicit operator Mail(MimeMessage mimeMessage)
        {
            if (mimeMessage == null)
            {
                throw new System.ArgumentNullException(nameof(mimeMessage));
            }

            Mail mail = new Mail();
            mail.Subject = mimeMessage.Subject;
            mail.SenderName = mimeMessage.From[0].Name;
            mail.Date = mimeMessage.Date.Date;
            mail.IsRead = new Random().Next(100) > 50; // TODO: fix it
            return mail;
        }
        //TODO: Add property for attachments
    }
}