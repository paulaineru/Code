using Microsoft.EntityFrameworkCore;
using SharedKernel.Models;
using Microsoft.Extensions.Configuration;
using PropertyManagementService.Models;

namespace PropertyManagementService.Repository
{
    public class PropertyDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public PropertyDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyFeature> PropertyFeatures { get; set; }
        public DbSet<PropertyAmenity> PropertyAmenities { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<WingDetails> WingDetails { get; set; }
        public DbSet<CondominiumUnit> CondominiumUnits { get; set; }
        public DbSet<TownhouseCluster> TownhouseClusters { get; set; }
        
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<PropertyCertification> PropertyCertifications { get; set; }
        public DbSet<PropertyCompliance> PropertyCompliance { get; set; }
        public DbSet<PropertyRegulation> PropertyRegulations { get; set; }
        public DbSet<PropertyStandard> PropertyStandards { get; set; }
        public DbSet<PropertyService> PropertyServices { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"), 
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "propertydb"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("propertydb");
            
            modelBuilder.Entity<Property>().ToTable("Prop_Properties");
            modelBuilder.Entity<PropertyImage>().ToTable("Prop_PropertyImages");
            modelBuilder.Entity<PropertyFeature>().ToTable("Prop_PropertyFeatures");
            modelBuilder.Entity<PropertyAmenity>().ToTable("Prop_PropertyAmenities");
            modelBuilder.Entity<MaintenanceRecord>().ToTable("Prop_MaintenanceRecords");
            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Property>()
                .HasDiscriminator<string>("PropertyType")
                .HasValue<CommercialProperty>("Commercial")
                .HasValue<CondominiumProperty>("Condominium")
                .HasValue<TownhouseProperty>("Townhouse")
                .HasValue<BungalowProperty>("Bungalow")
                .HasValue<VillaProperty>("Villa")
                .HasValue<VacantLandProperty>("VacantLand");

            modelBuilder.Entity<Property>()
                .HasKey(p => p.Id)
                .HasName("PK_Properties");

            modelBuilder.Entity<Property>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
            
            modelBuilder.Entity<Property>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Property>()
                .Property(p => p.Address)
                .IsRequired()
                .HasMaxLength(200);
            modelBuilder.Entity<Property>()
                .Property(p => p.YearOfCommissionOrPurchase)
                .IsRequired()
                .HasDefaultValueSql("EXTRACT(YEAR FROM CURRENT_TIMESTAMP)");

            modelBuilder.Entity<Property>()
                .Property(p => p.FairValue)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Property>()
                .Property(p => p.InsurableValue)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Property>()
                .Property(p => p.ApprovalStatus)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Property>()
                .Property(p => p.NumberOfStories)
                .HasDefaultValue(1);

            // Configure one-to-one relationships for property features
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Certifications)
                .WithOne(c => c.Property)
                .HasForeignKey<PropertyCertification>(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Compliance)
                .WithOne(c => c.Property)
                .HasForeignKey<PropertyCompliance>(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Regulations)
                .WithOne(r => r.Property)
                .HasForeignKey<PropertyRegulation>(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Standards)
                .WithOne(s => s.Property)
                .HasForeignKey<PropertyStandard>(s => s.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Features)
                .WithOne(f => f.Property)
                .HasForeignKey<PropertyFeatures>(f => f.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Services)
                .WithOne(s => s.Property)
                .HasForeignKey<PropertyService>(s => s.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure existing relationships
            modelBuilder.Entity<CommercialProperty>()
                .HasMany(c => c.Wings)
                .WithOne(w => w.Property)
                .HasForeignKey(w => w.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WingDetails>()
                .HasKey(w => w.Id)
                .HasName("PK_WingDetails");

            modelBuilder.Entity<WingDetails>()
                .Property(w => w.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<WingDetails>()
                .Property(w => w.PropertyId)
                .IsRequired();

            modelBuilder.Entity<WingDetails>()
                .Property(w => w.FloorNumber)
                .IsRequired();

            modelBuilder.Entity<CondominiumProperty>()
                .HasMany(c => c.Units)
                .WithOne(u => u.Property)
                .HasForeignKey(u => u.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CondominiumUnit>()
                .HasKey(u => u.Id)
                .HasName("PK_CondominiumUnits");

            modelBuilder.Entity<CondominiumUnit>()
                .Property(u => u.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<CondominiumUnit>()
                .Property(u => u.PropertyId)
                .IsRequired();

            modelBuilder.Entity<TownhouseProperty>()
                .HasMany(t => t.Clusters)
                .WithOne(c => c.Property)
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<TownhouseProperty>()
                .Property(t => t.NumberOfClusters)
                .IsRequired()
                .HasDefaultValue(0);

            modelBuilder.Entity<TownhouseCluster>()
                .HasKey(c => c.Id)
                .HasName("PK_TownhouseClusters");
            
            modelBuilder.Entity<TownhouseCluster>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TownhouseCluster>()
                .Property(c => c.PropertyId)
                .IsRequired();
            modelBuilder.Entity<TownhouseCluster>()
                .Property(c => c.ClusterName)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<BungalowProperty>()
                .Property(b => b.NumberOfBedrooms)
                .IsRequired();

            modelBuilder.Entity<BungalowProperty>()
                .Property(b => b.NumberOfBathrooms)
                .IsRequired();

            modelBuilder.Entity<VillaProperty>()
                .Property(v => v.NumberOfBedrooms)
                .IsRequired();

            modelBuilder.Entity<VillaProperty>()
                .Property(v => v.NumberOfBathrooms)
                .IsRequired();

            modelBuilder.Entity<VacantLandProperty>()
                .Property(v => v.BlockNumber)
                .IsRequired();

            modelBuilder.Entity<VacantLandProperty>()
                .Property(v => v.PlotNumber)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<VacantLandProperty>()
                .Property(v => v.Acreage)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Amenity>()
                .HasKey(a => a.Id)
                .HasName("PK_Amenities");

            modelBuilder.Entity<Amenity>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Amenity>()
                .Property(a => a.Name)
                .IsRequired();

            modelBuilder.Entity<Property>()
                .HasMany(p => p.Amenities)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "PropertyAmenities",
                    j => j.HasOne<Amenity>().WithMany().HasForeignKey("AmenityId"),
                    j => j.HasOne<Property>().WithMany().HasForeignKey("PropertyId"),
                    j =>
                    {
                        j.HasKey("PropertyId", "AmenityId");
                        j.ToTable("PropertyAmenities");
                        j.Property("PropertyId").IsRequired();
                        j.Property("AmenityId").IsRequired();
                    });
            modelBuilder.Entity<Property>()
                .HasIndex(p => p.OwnerId)
                .HasDatabaseName("IX_Properties_OwnerId");

            modelBuilder.Entity<Property>()
                .HasIndex(p => p.ApprovalStatus)
                .HasDatabaseName("IX_Properties_ApprovalStatus");

            modelBuilder.Entity<LeaseAgreement>()
                .HasIndex(la => la.TenantId)
                .HasDatabaseName("IX_LeaseAgreements_TenantId");

            modelBuilder.Entity<Amenity>()
                .HasIndex(a => a.Name)
                .IsUnique()
                .HasDatabaseName("IX_Amenities_Name");
        }
    }
}