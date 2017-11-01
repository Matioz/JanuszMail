using System.ComponentModel.DataAnnotations;

namespace JanuszMail.Models
{
    public class User
    {
        public enum ProviderType
        {
            [Display(Name = "gmail.com")]
            Google,
            [Display(Name = "hotmail.com")]
            Microsoft,
            [Display(Name = "student.pg.edu.pl")]
            MojaPG
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public ProviderType Provider { get; set; }
    }
}