using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace NotificationService.Repository
{
    public class DesignTimeNotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
            // var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
            // var configuration = new ConfigurationBuilder()
            //     .SetBasePath(Directory.GetCurrentDirectory())
            //     .AddJsonFile("appsettings.json")
            //     .Build();
            // optionsBuilder.UseNpgsql(configuration.GetConnectionString("NotificationDb"));
            // return new NotificationDbContext(optionsBuilder.Options);
            return new NotificationDbContext();
        }
    }
} 