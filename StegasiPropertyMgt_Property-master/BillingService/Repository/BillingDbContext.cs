// BillingService/Data/BillingDbContext.cs
using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using System;
using Npgsql;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class BillingDbContext : DbContext
{
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Parameterless constructor for manual use
    public BillingDbContext() { }

    // Constructor for dependency injection
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
        // Log connection string details
        var connectionString = Database.GetConnectionString();
        if (connectionString != null)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                var sanitizedConnectionString = connectionString.Contains("Password=") 
                    ? connectionString.Replace(connectionString.Split("Password=")[1], "Password=***") 
                    : connectionString;

                Console.WriteLine($"[BillingDbContext] Initializing with connection string: {sanitizedConnectionString}");
                Console.WriteLine($"[BillingDbContext] Connection details:");
                Console.WriteLine($"  - Host: {builder.Host}");
                Console.WriteLine($"  - Port: {builder.Port}");
                Console.WriteLine($"  - Database: {builder.Database}");
                Console.WriteLine($"  - Username: {builder.Username}");
                Console.WriteLine($"  - Timeout: {builder.Timeout} seconds");
                Console.WriteLine($"  - SSL Mode: {builder.SslMode}");
                Console.WriteLine($"  - Pooling Enabled: {builder.Pooling}");
                Console.WriteLine($"  - Min Pool Size: {builder.MinPoolSize}");
                Console.WriteLine($"  - Max Pool Size: {builder.MaxPoolSize}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BillingDbContext Error] Failed to parse connection string: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("[BillingDbContext Warning] No connection string available");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            Console.WriteLine("[BillingDbContext Warning] DbContext not configured through constructor");
            return;
        }

        // Add detailed logging
        optionsBuilder
            .LogTo(message => Console.WriteLine($"[EF Core] {message}"))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine("[BillingDbContext] Attempting to save changes...");
            var result = await base.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"[BillingDbContext] Successfully saved {result} changes");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BillingDbContext Error] Failed to save changes: {ex.Message}");
            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billingdb");
        base.OnModelCreating(modelBuilder);

        // Configure Invoice entity
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id); // Define primary key
            entity.Property(i => i.Amount).IsRequired();
            entity.Property(i => i.DueDate).IsRequired();
            entity.HasMany(i => i.Payments).WithOne(p => p.Invoice).HasForeignKey(p => p.InvoiceId); // One-to-many relationship
        });

        // Configure Payment entity
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id); // Define primary key
            entity.Property(p => p.AmountPaid).IsRequired();
            entity.HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId);
        });
    }
}