namespace JanuszMail.Models
{
    public class Mail
    {
        public int ID { get; set; }
        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        //TODO: Add property for attachments
    }
}