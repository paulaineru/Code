using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using Npgsql;

public class DesignTimeBillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BillingDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Log connection string and current directory
        var sanitizedConnectionString = connectionString?.Replace(configuration["ConnectionStrings:BillingDb"]?.Split("Password=")[1] ?? "", "Password=***");
        Console.WriteLine($"[Connection Info] Using connection string: {sanitizedConnectionString}");
        Console.WriteLine($"[Connection Info] Current directory: {Directory.GetCurrentDirectory()}");

        // Parse and log connection string details
        if (connectionString != null)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                Console.WriteLine($"[Connection Details] Host: {builder.Host}");
                Console.WriteLine($"[Connection Details] Port: {builder.Port}");
                Console.WriteLine($"[Connection Details] Database: {builder.Database}");
                Console.WriteLine($"[Connection Details] Username: {builder.Username}");
                Console.WriteLine($"[Connection Details] Connection Timeout: {builder.Timeout} seconds");
                Console.WriteLine($"[Connection Details] SSL Mode: {builder.SslMode}");
                Console.WriteLine($"[Connection Details] Pooling: {builder.Pooling}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to parse connection string: {ex.Message}");
            }
        }

        try
        {
            Console.WriteLine("[Connection Attempt] Creating connection options...");
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                options.CommandTimeout(30);
            });
            Console.WriteLine("[Connection Attempt] Connection options created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Failed to create connection options: {ex.Message}");
            throw;
        }

        return new BillingDbContext(optionsBuilder.Options);
    }
} 