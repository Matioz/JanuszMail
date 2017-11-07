using System.ComponentModel.DataAnnotations;

namespace JanuszMail.Models
{
    public class Contact
    {
        public int ID { get; set; }
        public string Name { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public ProviderParams Provider { get; set; }
    }
}