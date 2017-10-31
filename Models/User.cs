using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using JanuszMail.Models;
using System.Collections.Generic;


namespace JanuszMail.Models
{
    public class User
    {
        public User()
        {
        }

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
        public virtual ICollection<ProviderModel> Providers { get; set; }
    }
}