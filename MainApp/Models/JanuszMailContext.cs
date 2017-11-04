using Microsoft.EntityFrameworkCore;

namespace JanuszMail.Models
{
    public class JanuszMailDbContext : DbContext
    {
        public JanuszMailDbContext(DbContextOptions<JanuszMailDbContext> options)
        : base(options)
        {
        }

        public DbSet<ProviderParams> ProviderParams { get; set; }
    }
}