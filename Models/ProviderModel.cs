using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace JanuszMail.Models
{
    public class ProviderModel
    {
        public ProviderModel()
        {
        }

        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Imap { get; set; }
        public string Smtp { get; set; }
        public int? UserId {get;set;}

    }
}