
namespace JanuszMail.Models
{
    public class ProviderParams
    {
        public int ID { get; set; }
        public string EmailAdress { get; set; }
        public string Password { get; set; }
        public string ImapServerName { get; set; }
        public int ImapPortNumber { get; set; }
        public string SmtpServerName { get; set; }
        public int SmtpPortNumber { get; set; }
        public int? UserId { get; set; }
    }
}
