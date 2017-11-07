using JanuszMail.Models;
using Microsoft.EntityFrameworkCore;

namespace JanuszMail.Data
{
    public class JanuszMailDbContext : DbContext
    {
        public JanuszMailDbContext(DbContextOptions<JanuszMailDbContext> options)
        : base(options)
        {
        }

        public DbSet<ProviderParams> ProviderParams { get; set; }
        public DbSet<Contact> Contact { get; set; }
    }
}