using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedKernel.Models;
using System.Text.Json;

namespace SharedKernel.Data
{
    public class ApprovalWorkflowDbContext : DbContext
    {
        public ApprovalWorkflowDbContext(DbContextOptions<ApprovalWorkflowDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }
        public DbSet<ApprovalStage> ApprovalStages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("approvalworkflow");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApprovalWorkflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Module).IsRequired();
                entity.Property(e => e.EntityType).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();

                // Configure the relationship with stages
                entity.HasMany(e => e.Stages)
                    .WithOne()
                    .HasForeignKey("WorkflowId")
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure Metadata as a JSON string with value comparer
                entity.Property(e => e.Metadata)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, object>()
                    )
                    .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => new Dictionary<string, object>(c)
                    ));
            });

            modelBuilder.Entity<ApprovalStage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StageNumber).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Order).IsRequired();
                entity.Property(e => e.IsRequired).IsRequired();
            });
        }
    }
} 