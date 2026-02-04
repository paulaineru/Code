using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PropertyManagementService.Repository;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PropertyDbContext>
{
    public PropertyDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        return new PropertyDbContext(configuration);
    }
} 