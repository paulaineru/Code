using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedKernel.Models
{
    public class ApprovalWorkflow
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Module { get; set; } // e.g., "Property", "Lease", "Payment", "Sale"
        [Required]
        public Guid EntityId { get; set; } // ID of the entity being approved
        [Required]
        public string EntityType { get; set; } // Type of entity (e.g., "Property", "LeaseAgreement")
        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
        [Required]
        public Guid CreatedBy { get; set; }
        public List<ApprovalStage> Stages { get; set; } = new();
        public string? Comments { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new(); // Additional module-specific data
    }

    public class ApprovalStage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public int StageNumber { get; set; }
        [Required]
        public string Role { get; set; } // Role required for this stage
        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string? Comments { get; set; }
        [Required]
        public bool IsRequired { get; set; } = true;
        [Required]
        public int Order { get; set; } // Order in the workflow
    }

    public class ApprovalHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Guid EntityId { get; set; } // e.g., PropertyId, LeaseAgreementId
        [Required]
        public string EntityType { get; set; } = string.Empty;
        [Required]
        public Guid StageId { get; set; }
        [ForeignKey("StageId")]
        public virtual ApprovalStage Stage { get; set; } = null!;
        [Required]
        public Guid PerformedByUserId { get; set; }
        [Required]
        public ApprovalStatus Status { get; set; }
        public string? Comments { get; set; }
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ApprovalStatus
    {
        Pending,
        InProgress,
        Approved,
        Rejected,
        MoreInfoRequired,
        Cancelled
    }

    // Module-specific approval configurations
    public static class ApprovalConfigurations
    {
        public static readonly Dictionary<string, List<ApprovalStage>> ModuleWorkflows = new()
        {
            {
                "Property", new List<ApprovalStage>
                {
                    new() { StageNumber = 1, Role = "Estates Officer", Order = 1 },
                    new() { StageNumber = 2, Role = "Property Manager", Order = 2 }
                }
            },
            {
                "Lease", new List<ApprovalStage>
                {
                    new() { StageNumber = 1, Role = "Property Manager", Order = 1 },
                    new() { StageNumber = 2, Role = "Estates Officer", Order = 2 }
                }
            },
            {
                "Payment", new List<ApprovalStage>
                {
                    new() { StageNumber = 1, Role = "Finance Team", Order = 1 },
                    new() { StageNumber = 2, Role = "Property Manager", Order = 2 },
                    new() { StageNumber = 3, Role = "Property Manager", Order = 3 }
                }
            },
            {
                "Sale", new List<ApprovalStage>
                {
                    new() { StageNumber = 1, Role = "Sales Officer", Order = 1 },
                    new() { StageNumber = 2, Role = "Sales Manager", Order = 2 },
                    new() { StageNumber = 3, Role = "Property Manager", Order = 3 }
                }
            },
            {
                "Termination", new List<ApprovalStage>
                {
                    new() { StageNumber = 1, Role = "Property Manager", Order = 1 },
                    new() { StageNumber = 2, Role = "Estates Officer", Order = 2 }
                }
            }
        };

        // Lease approval workflow
        public static List<ApprovalStage> GetLeaseApprovalStages()
        {
            return new List<ApprovalStage>
            {
                new() { StageNumber = 1, Role = "Property Manager", Order = 1 },
                new() { StageNumber = 2, Role = "Estates Officer", Order = 2 }
            };
        }

        // Payment approval workflow
        public static List<ApprovalStage> GetPaymentApprovalStages()
        {
            return new List<ApprovalStage>
            {
                new() { StageNumber = 1, Role = "Finance Team", Order = 1 },
                new() { StageNumber = 2, Role = "Property Manager", Order = 2 },
                new() { StageNumber = 3, Role = "Property Manager", Order = 3 }
            };
        }

        // Maintenance approval workflow
        public static List<ApprovalStage> GetMaintenanceApprovalStages()
        {
            return new List<ApprovalStage>
            {
                new() { StageNumber = 1, Role = "Property Manager", Order = 1 },
                new() { StageNumber = 2, Role = "Estates Officer", Order = 2 }
            };
        }
    }
} 