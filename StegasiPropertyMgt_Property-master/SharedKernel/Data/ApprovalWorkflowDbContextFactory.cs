using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SharedKernel.Data
{
    public class ApprovalWorkflowDbContextFactory : IDesignTimeDbContextFactory<ApprovalWorkflowDbContext>
    {
        public ApprovalWorkflowDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApprovalWorkflowDbContext>();
            var connectionString = configuration.GetConnectionString("ApprovalWorkflowDb");
            optionsBuilder.UseNpgsql(connectionString);

            return new ApprovalWorkflowDbContext(optionsBuilder.Options);
        }
    }
} 