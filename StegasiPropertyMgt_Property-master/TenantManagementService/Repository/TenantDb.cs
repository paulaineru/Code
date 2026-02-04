// TenantManagementService/Data/TenantDbContext.cs

using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using Microsoft.Extensions.Configuration;
using SharedKernel.Models.Tenants;
using Npgsql;

namespace TenantManagementService.Repository
{
    public class TenantDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public TenantDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            try
            {
                using var conn = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                conn.Open();

                // Create schema
                using (var cmd = new NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS tenantdb;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // Create migrations history table
                using (var cmd = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS tenantdb.""__EFMigrationsHistory"" (
                        ""MigrationId"" character varying(150) NOT NULL,
                        ""ProductVersion"" character varying(32) NOT NULL,
                        CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                    );", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring database setup: {ex.Message}");
            }
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<RenewalRequest> RenewalRequests { get; set; }
        public DbSet<TerminationProcess> TerminationProcesses { get; set; }
        public DbSet<InspectionReport> InspectionReports { get; set; }
        public DbSet<LeaseAgreement> LeaseAgreements { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"), 
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "tenantdb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("tenantdb");
            
            modelBuilder.Entity<Tenant>().ToTable("Tenant_Tenants");
            modelBuilder.Entity<LeaseAgreement>().ToTable("Tenant_LeaseAgreements");
            modelBuilder.Entity<RenewalRequest>().ToTable("Tenant_RenewalRequests");
            modelBuilder.Entity<TerminationProcess>().ToTable("Tenant_TerminationProcesses");
            modelBuilder.Entity<Booking>().ToTable("Tenant_Bookings");
            modelBuilder.Entity<Invoice>().ToTable("Tenant_Invoices");
            
            base.OnModelCreating(modelBuilder);
            // Configure Tenant entity
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Name).IsRequired();
                entity.Property(t => t.PrimaryEmail).IsRequired();
                // One-to-many relationship with LeaseAgreement
                entity.HasMany(t => t.LeaseAgreements) // Add this line
                    .WithOne(la => la.Tenant) // Add this line
                    .HasForeignKey(la => la.TenantId); // Add this line
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasOne(b => b.Tenant).WithMany(t => t.Bookings).HasForeignKey(b => b.TenantId);
                //entity.HasOne(b => b.Property).WithMany().HasForeignKey(b => b.PropertyId);
            });

            // Configure RenewalRequest entity
            modelBuilder.Entity<RenewalRequest>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasOne(r => r.Tenant).WithMany(t => t.RenewalRequests).HasForeignKey(r => r.TenantId);
            });

            // Configure TerminationProcess entity
            modelBuilder.Entity<TerminationProcess>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.HasOne(t => t.LeaseAgreement).WithOne(la => la.TerminationProcess).HasForeignKey<TerminationProcess>(t => t.LeaseAgreementId); // One-to-one relationship
                entity.HasOne(t => t.Tenant).WithMany(tn => tn.TerminationProcesses).HasForeignKey(t => t.TenantId);
            });

            // Configure InspectionReport entity
            modelBuilder.Entity<InspectionReport>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.HasOne(i => i.TerminationProcess).WithMany(tp => tp.InspectionReports).HasForeignKey(i => i.TerminationProcessId);
            });

            // Configure LeaseAgreement entity
            modelBuilder.Entity<LeaseAgreement>(entity =>
            {
                entity.HasKey(la => la.Id);
                //entity.HasOne(la => la.Property).WithMany(p => p.LeaseAgreements).HasForeignKey(la => la.PropertyId); // One-to-many relationship with Property
                entity.HasOne(la => la.Tenant).WithMany(t => t.LeaseAgreements).HasForeignKey(la => la.TenantId); // One-to-many relationship with Tenant
            });
        }
    }
}