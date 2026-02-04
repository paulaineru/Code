using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ReportingService.Data
{
    public class DesignTimeReportingDbContextFactory : IDesignTimeDbContextFactory<ReportingDbContext>
    {
        public ReportingDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ReportingDbContext>();
            var connectionString = configuration.GetConnectionString("ReportingDb");
            optionsBuilder.UseNpgsql(connectionString);

            return new ReportingDbContext(optionsBuilder.Options);
        }
    }
} 