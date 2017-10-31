using Microsoft.EntityFrameworkCore;

namespace JanuszMail.Models
{
    public class JanuszMailContext : DbContext
    {
        public JanuszMailContext(DbContextOptions<JanuszMailContext> options)
        : base(options)
        {
        }

        public DbSet<JanuszMail.Models.User> User { get; set; }
        public DbSet<JanuszMail.Models.ProviderModel> Providers { get; set; }

    }
}