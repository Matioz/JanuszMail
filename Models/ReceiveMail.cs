using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;

namespace JanuszMail.Models
{
    public static class ReceiveMail
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new JanuszMailContext(serviceProvider.GetRequiredService<DbContextOptions<JanuszMailContext>>()))
            {
                using (var client = new ImapClient ()) 
                {
                    // For demo-purposes, accept all SSL certificates
                    client.ServerCertificateValidationCallback = (s,c,h,e) => true;

                    client.Connect ("imap.gmail.com", 993, true);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove ("XOAUTH2");

                    client.Authenticate ("januszmail2137@gmail.com", "januszmail1");

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open (FolderAccess.ReadOnly);

                    for (int i = 0; i < inbox.Count; i++) 
                    {
                        var message = inbox.GetMessage (i);
                        context.Email.Add(
                            new Email {Subject = message.Subject, Body = message.GetTextBody(MimeKit.Text.TextFormat.Plain)}
                        );
                    }
                    client.Disconnect (true);
                    context.SaveChanges();
                }
            }
        }
    }
}