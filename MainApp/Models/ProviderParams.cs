using System.ComponentModel.DataAnnotations;

namespace JanuszMail.Models
{
    public class ProviderParams
    {
        public int ID { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAdress { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ImapServerName { get; set; }
        [Required]
        public int ImapPortNumber { get; set; }
        [Required]
        public string SmtpServerName { get; set; }
        [Required]
        public int SmtpPortNumber { get; set; }
        public string UserId { get; set; }
    }
}