using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using SharedKernel.Models.Billing;

namespace ReportingService.Data
{
    public class ReportingDbContext : DbContext
    {
        public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bill> Bills { get; set; }
        public DbSet<MaintenanceTicket> MaintenanceTickets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("reporting");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Bill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.IsPaid).HasDefaultValue(false);
            });

            modelBuilder.Entity<MaintenanceTicket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RepairCost).HasPrecision(18, 2);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.Details).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
            });
        }
    }
} 