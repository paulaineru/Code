using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using Microsoft.Extensions.Configuration;
using SharedKernel.Models.Billing;

namespace BillingService.Data
{
    public class BillingDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public BillingDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<Bill> Bills { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"), 
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "billingdb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("billingdb");
            
            modelBuilder.Entity<Bill>().ToTable("Billing_Bills");
            modelBuilder.Entity<Payment>().ToTable("Billing_Payments");
            
            base.OnModelCreating(modelBuilder);
        }
    }
} 