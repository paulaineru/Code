using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;

namespace DocumentManagementService.Data
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
        {
        }

        // Document management tables
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<DocumentCategory> DocumentCategories { get; set; }
        public DbSet<DocumentSubCategory> DocumentSubCategories { get; set; }
        public DbSet<DocumentRequirement> DocumentRequirements { get; set; }
        public DbSet<PropertyDocument> PropertyDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default schema for all entities
            modelBuilder.HasDefaultSchema("document_management");

            // DocumentType configuration
            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.DisplayOrder).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Indexes
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // DocumentCategory configuration
            modelBuilder.Entity<DocumentCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentTypeId).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.IsRequired).IsRequired();
                entity.Property(e => e.DisplayOrder).IsRequired();
                entity.Property(e => e.ValidationRules).HasMaxLength(1000);
                entity.Property(e => e.AllowedFileTypes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Relationships
                entity.HasOne(e => e.DocumentType)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(e => e.DocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Indexes
                entity.HasIndex(e => new { e.DocumentTypeId, e.Name }).IsUnique();
                entity.HasIndex(e => e.Code);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // DocumentSubCategory configuration
            modelBuilder.Entity<DocumentSubCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentCategoryId).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.IsRequired).IsRequired();
                entity.Property(e => e.DisplayOrder).IsRequired();
                entity.Property(e => e.ValidationRules).HasMaxLength(1000);
                entity.Property(e => e.AllowedFileTypes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Relationships
                entity.HasOne(e => e.DocumentCategory)
                    .WithMany(e => e.SubCategories)
                    .HasForeignKey(e => e.DocumentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Indexes
                entity.HasIndex(e => new { e.DocumentCategoryId, e.Name }).IsUnique();
                entity.HasIndex(e => e.Code);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // DocumentRequirement configuration
            modelBuilder.Entity<DocumentRequirement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProcessType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SubProcess).HasMaxLength(100);
                entity.Property(e => e.DocumentTypeId).IsRequired();
                entity.Property(e => e.DocumentCategoryId).IsRequired();
                entity.Property(e => e.IsRequired).IsRequired();
                entity.Property(e => e.DisplayOrder).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ValidationRules).HasMaxLength(1000);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Relationships
                entity.HasOne(e => e.DocumentType)
                    .WithMany()
                    .HasForeignKey(e => e.DocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.DocumentCategory)
                    .WithMany()
                    .HasForeignKey(e => e.DocumentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.DocumentSubCategory)
                    .WithMany()
                    .HasForeignKey(e => e.DocumentSubCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Indexes
                entity.HasIndex(e => new { e.ProcessType, e.SubProcess, e.DocumentTypeId, e.DocumentCategoryId, e.DocumentSubCategoryId }).IsUnique();
                entity.HasIndex(e => e.ProcessType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // PropertyDocument configuration
            modelBuilder.Entity<PropertyDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentTypeId).IsRequired();
                entity.Property(e => e.DocumentCategoryId).IsRequired();
                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.FileUrl).IsRequired();
                entity.Property(e => e.ContentType).IsRequired();
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.UploadedAt).IsRequired();
                entity.Property(e => e.UploadedBy).IsRequired();
                entity.Property(e => e.IsRequired).IsRequired();
                entity.Property(e => e.IsVerified).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Version).IsRequired();
                entity.Property(e => e.Tags);
                entity.Property(e => e.Description);
                entity.Property(e => e.Comments);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Relationships
                entity.HasOne(e => e.DocumentType)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.DocumentTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.DocumentCategory)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.DocumentCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.DocumentSubCategory)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.DocumentSubCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Indexes
                entity.HasIndex(e => e.DocumentTypeId);
                entity.HasIndex(e => e.DocumentCategoryId);
                entity.HasIndex(e => e.DocumentSubCategoryId);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.LeaseId);
                entity.HasIndex(e => e.MaintenanceTicketId);
                entity.HasIndex(e => e.BookingId);
                entity.HasIndex(e => e.RenewalRequestId);
                entity.HasIndex(e => e.TerminationProcessId);
                entity.HasIndex(e => e.BillId);
                entity.HasIndex(e => e.PaymentId);
                entity.HasIndex(e => e.UploadedBy);
                entity.HasIndex(e => e.VerifiedBy);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsVerified);
                entity.HasIndex(e => e.IsRequired);
                entity.HasIndex(e => e.ExpiryDate);
                entity.HasIndex(e => e.UploadedAt);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
} 