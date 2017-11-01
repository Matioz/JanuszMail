using System.ComponentModel.DataAnnotations;

namespace JanuszMail.Models
{
    public class Email
    {
        [Key]
        public int ID { get; set; }
        public string Subject { get; set; }
        public string Body {get; set;}
    }
}