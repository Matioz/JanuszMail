using System.ComponentModel.DataAnnotations;

namespace JanuszMail.Models
{
    public class Contact
    {
        public int ID { get; set; }
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
        public string UserId { get; set; }
    }
}