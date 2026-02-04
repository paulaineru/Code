using Microsoft.EntityFrameworkCore;
// NotificationService/Data/NotificationDbContext.cs
namespace NotificationService.Repository
{
    public class NotificationDbContext : DbContext
    {
        /*
        public DbSet<Email> Emails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=postgres;Database=notificationdb;Username=user;Password=password");
        }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("notification");
            base.OnModelCreating(modelBuilder);
        }
    }
}