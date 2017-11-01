using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace JanuszMail.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new JanuszMailContext(serviceProvider.GetRequiredService<DbContextOptions<JanuszMailContext>>()))
            {
                if (context.User.Any())
                {
                    return; // DB already has been seed.
                }

                context.User.AddRange(
                    new User
                    {
                        Name = "Zenek Martyniuk",
                        Provider = User.ProviderType.Google
                    },
                    new User
                    {
                        Name = "Doda Elektroda",
                        Provider = User.ProviderType.Microsoft
                    },
                    new User
                    {
                        Name = "Jan Daciuk",
                        Provider = User.ProviderType.MojaPG
                    }
                );

                context.SaveChanges();
            }
        }
    }
}