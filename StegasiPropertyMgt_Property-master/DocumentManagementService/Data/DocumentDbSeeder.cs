using SharedKernel.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementService.Data
{
    public static class DocumentDbSeeder
    {
        public static async Task SeedAsync(DocumentDbContext context)
        {
            if (!context.DocumentTypes.Any())
            {
                await SeedDocumentTypesAsync(context);
                await context.SaveChangesAsync();
            }

            if (!context.DocumentCategories.Any())
            {
                await SeedDocumentCategoriesAsync(context);
                await context.SaveChangesAsync();
            }

            if (!context.DocumentSubCategories.Any())
            {
                await SeedDocumentSubCategoriesAsync(context);
                await context.SaveChangesAsync();
            }

            if (!context.DocumentRequirements.Any())
            {
                await SeedDocumentRequirementsAsync(context);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDocumentTypesAsync(DocumentDbContext context)
        {
            var documentTypes = new List<DocumentType>
            {
                new DocumentType { Name = "Property", Description = "Property-related documents", DisplayOrder = 1, IsActive = true },
                new DocumentType { Name = "Tenant", Description = "Tenant-related documents", DisplayOrder = 2, IsActive = true },
                new DocumentType { Name = "Lease", Description = "Lease agreement documents", DisplayOrder = 3, IsActive = true },
                new DocumentType { Name = "Maintenance", Description = "Maintenance and repair documents", DisplayOrder = 4, IsActive = true },
                new DocumentType { Name = "Financial", Description = "Financial and billing documents", DisplayOrder = 5, IsActive = true },
                new DocumentType { Name = "Legal", Description = "Legal and compliance documents", DisplayOrder = 6, IsActive = true },
                new DocumentType { Name = "Insurance", Description = "Insurance-related documents", DisplayOrder = 7, IsActive = true },
                new DocumentType { Name = "Compliance", Description = "Regulatory compliance documents", DisplayOrder = 8, IsActive = true },
                new DocumentType { Name = "Marketing", Description = "Marketing and promotional materials", DisplayOrder = 9, IsActive = true },
                new DocumentType { Name = "Operations", Description = "Operational and administrative documents", DisplayOrder = 10, IsActive = true },
                new DocumentType { Name = "HR", Description = "Human resources documents", DisplayOrder = 11, IsActive = true }
            };

            await context.DocumentTypes.AddRangeAsync(documentTypes);
        }

        private static async Task SeedDocumentCategoriesAsync(DocumentDbContext context)
        {
            var propertyType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Property");
            var tenantType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Tenant");
            var leaseType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Lease");
            var maintenanceType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Maintenance");
            var financialType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Financial");
            var legalType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Legal");
            var insuranceType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Insurance");

            var categories = new List<DocumentCategory>
            {
                // Property Categories
                new DocumentCategory { DocumentTypeId = propertyType.Id, Name = "Ownership", Description = "Property ownership documents", Code = "PROP_OWN", DisplayOrder = 1, IsRequired = true, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = propertyType.Id, Name = "Construction", Description = "Construction and building documents", Code = "PROP_CONST", DisplayOrder = 2, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = propertyType.Id, Name = "Inspection", Description = "Property inspection reports", Code = "PROP_INSP", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = propertyType.Id, Name = "Amenities", Description = "Property amenities documentation", Code = "PROP_AMEN", DisplayOrder = 4, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 10485760 },

                // Tenant Categories
                new DocumentCategory { DocumentTypeId = tenantType.Id, Name = "Application", Description = "Tenant application forms", Code = "TEN_APP", DisplayOrder = 1, IsRequired = true, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = tenantType.Id, Name = "Identity", Description = "Identity verification documents", Code = "TEN_ID", DisplayOrder = 2, IsRequired = true, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = tenantType.Id, Name = "Financial", Description = "Financial verification documents", Code = "TEN_FIN", DisplayOrder = 3, IsRequired = true, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = tenantType.Id, Name = "References", Description = "Reference letters and contacts", Code = "TEN_REF", DisplayOrder = 4, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = tenantType.Id, Name = "Employment", Description = "Employment verification documents", Code = "TEN_EMP", DisplayOrder = 5, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },

                // Lease Categories
                new DocumentCategory { DocumentTypeId = leaseType.Id, Name = "Agreement", Description = "Lease agreement documents", Code = "LEASE_AGR", DisplayOrder = 1, IsRequired = true, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = leaseType.Id, Name = "Condition", Description = "Property condition reports", Code = "LEASE_COND", DisplayOrder = 2, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = leaseType.Id, Name = "Payment", Description = "Payment and deposit documents", Code = "LEASE_PAY", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = leaseType.Id, Name = "Termination", Description = "Lease termination documents", Code = "LEASE_TERM", DisplayOrder = 4, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = leaseType.Id, Name = "Renewal", Description = "Lease renewal documents", Code = "LEASE_RENEW", DisplayOrder = 5, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },

                // Maintenance Categories
                new DocumentCategory { DocumentTypeId = maintenanceType.Id, Name = "Request", Description = "Maintenance request forms", Code = "MAINT_REQ", DisplayOrder = 1, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = maintenanceType.Id, Name = "Report", Description = "Maintenance reports", Code = "MAINT_REP", DisplayOrder = 2, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = maintenanceType.Id, Name = "Invoice", Description = "Maintenance invoices", Code = "MAINT_INV", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },

                // Financial Categories
                new DocumentCategory { DocumentTypeId = financialType.Id, Name = "Invoice", Description = "Billing invoices", Code = "FIN_INV", DisplayOrder = 1, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = financialType.Id, Name = "Receipt", Description = "Payment receipts", Code = "FIN_REC", DisplayOrder = 2, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = financialType.Id, Name = "Statement", Description = "Financial statements", Code = "FIN_STMT", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = financialType.Id, Name = "Tax", Description = "Tax-related documents", Code = "FIN_TAX", DisplayOrder = 4, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },

                // Legal Categories
                new DocumentCategory { DocumentTypeId = legalType.Id, Name = "Contract", Description = "Legal contracts", Code = "LEGAL_CON", DisplayOrder = 1, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = legalType.Id, Name = "Permit", Description = "Permits and licenses", Code = "LEGAL_PERM", DisplayOrder = 2, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = legalType.Id, Name = "Certificate", Description = "Certificates and compliance", Code = "LEGAL_CERT", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentCategory { DocumentTypeId = legalType.Id, Name = "Litigation", Description = "Legal proceedings documents", Code = "LEGAL_LIT", DisplayOrder = 4, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },

                // Insurance Categories
                new DocumentCategory { DocumentTypeId = insuranceType.Id, Name = "Policy", Description = "Insurance policies", Code = "INS_POL", DisplayOrder = 1, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = insuranceType.Id, Name = "Claim", Description = "Insurance claims", Code = "INS_CLAIM", DisplayOrder = 2, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },
                new DocumentCategory { DocumentTypeId = insuranceType.Id, Name = "Assessment", Description = "Insurance assessments", Code = "INS_ASSESS", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 }
            };

            await context.DocumentCategories.AddRangeAsync(categories);
        }

        private static async Task SeedDocumentSubCategoriesAsync(DocumentDbContext context)
        {
            var ownershipCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "PROP_OWN");
            var identityCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "TEN_ID");
            var financialCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "TEN_FIN");
            var agreementCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "LEASE_AGR");

            var subCategories = new List<DocumentSubCategory>
            {
                // Property Ownership Sub-Categories
                new DocumentSubCategory { DocumentCategoryId = ownershipCategory.Id, Name = "Title Deed", Description = "Property title deed", Code = "OWN_TITLE", DisplayOrder = 1, IsRequired = true, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },
                new DocumentSubCategory { DocumentCategoryId = ownershipCategory.Id, Name = "Survey Plan", Description = "Property survey plan", Code = "OWN_SURVEY", DisplayOrder = 2, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 10485760 },
                new DocumentSubCategory { DocumentCategoryId = ownershipCategory.Id, Name = "Certificate of Occupancy", Description = "Certificate of occupancy", Code = "OWN_COO", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },

                // Tenant Identity Sub-Categories
                new DocumentSubCategory { DocumentCategoryId = identityCategory.Id, Name = "National ID", Description = "National identification card", Code = "ID_NATIONAL", DisplayOrder = 1, IsRequired = true, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 5242880 },
                new DocumentSubCategory { DocumentCategoryId = identityCategory.Id, Name = "Passport", Description = "Passport document", Code = "ID_PASSPORT", DisplayOrder = 2, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 5242880 },
                new DocumentSubCategory { DocumentCategoryId = identityCategory.Id, Name = "Driver's License", Description = "Driver's license", Code = "ID_DRIVER", DisplayOrder = 3, AllowedFileTypes = "pdf,jpg,jpeg,png", MaxFileSizeInBytes = 5242880 },

                // Tenant Financial Sub-Categories
                new DocumentSubCategory { DocumentCategoryId = financialCategory.Id, Name = "Bank Statement", Description = "Bank account statements", Code = "FIN_BANK", DisplayOrder = 1, IsRequired = true, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentSubCategory { DocumentCategoryId = financialCategory.Id, Name = "Payslip", Description = "Employment payslips", Code = "FIN_PAYSLIP", DisplayOrder = 2, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },
                new DocumentSubCategory { DocumentCategoryId = financialCategory.Id, Name = "Tax Return", Description = "Tax return documents", Code = "FIN_TAX_RETURN", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 5242880 },

                // Lease Agreement Sub-Categories
                new DocumentSubCategory { DocumentCategoryId = agreementCategory.Id, Name = "Standard Lease", Description = "Standard lease agreement", Code = "AGR_STANDARD", DisplayOrder = 1, IsRequired = true, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },
                new DocumentSubCategory { DocumentCategoryId = agreementCategory.Id, Name = "Commercial Lease", Description = "Commercial property lease", Code = "AGR_COMMERCIAL", DisplayOrder = 2, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 },
                new DocumentSubCategory { DocumentCategoryId = agreementCategory.Id, Name = "Residential Lease", Description = "Residential property lease", Code = "AGR_RESIDENTIAL", DisplayOrder = 3, AllowedFileTypes = "pdf", MaxFileSizeInBytes = 10485760 }
            };

            await context.DocumentSubCategories.AddRangeAsync(subCategories);
        }

        private static async Task SeedDocumentRequirementsAsync(DocumentDbContext context)
        {
            var propertyType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Property");
            var tenantType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Tenant");
            var leaseType = await context.DocumentTypes.FirstAsync(dt => dt.Name == "Lease");

            var ownershipCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "PROP_OWN");
            var applicationCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "TEN_APP");
            var identityCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "TEN_ID");
            var financialCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "TEN_FIN");
            var agreementCategory = await context.DocumentCategories.FirstAsync(dc => dc.Code == "LEASE_AGR");

            var titleDeedSubCategory = await context.DocumentSubCategories.FirstAsync(dsc => dsc.Code == "OWN_TITLE");
            var nationalIdSubCategory = await context.DocumentSubCategories.FirstAsync(dsc => dsc.Code == "ID_NATIONAL");
            var bankStatementSubCategory = await context.DocumentSubCategories.FirstAsync(dsc => dsc.Code == "FIN_BANK");
            var standardLeaseSubCategory = await context.DocumentSubCategories.FirstAsync(dsc => dsc.Code == "AGR_STANDARD");

            var requirements = new List<DocumentRequirement>
            {
                // Property Registration Requirements
                new DocumentRequirement { ProcessType = "PropertyRegistration", DocumentTypeId = propertyType.Id, DocumentCategoryId = ownershipCategory.Id, DocumentSubCategoryId = titleDeedSubCategory.Id, IsRequired = true, DisplayOrder = 1, Description = "Title deed is required for property registration" },

                // Tenant Application Requirements
                new DocumentRequirement { ProcessType = "TenantApplication", DocumentTypeId = tenantType.Id, DocumentCategoryId = applicationCategory.Id, IsRequired = true, DisplayOrder = 1, Description = "Tenant application form is required" },
                new DocumentRequirement { ProcessType = "TenantApplication", DocumentTypeId = tenantType.Id, DocumentCategoryId = identityCategory.Id, DocumentSubCategoryId = nationalIdSubCategory.Id, IsRequired = true, DisplayOrder = 2, Description = "National ID is required for tenant verification" },
                new DocumentRequirement { ProcessType = "TenantApplication", DocumentTypeId = tenantType.Id, DocumentCategoryId = financialCategory.Id, DocumentSubCategoryId = bankStatementSubCategory.Id, IsRequired = true, DisplayOrder = 3, Description = "Bank statements are required for financial verification" },

                // Lease Creation Requirements
                new DocumentRequirement { ProcessType = "LeaseCreation", DocumentTypeId = leaseType.Id, DocumentCategoryId = agreementCategory.Id, DocumentSubCategoryId = standardLeaseSubCategory.Id, IsRequired = true, DisplayOrder = 1, Description = "Lease agreement is required for lease creation" }
            };

            await context.DocumentRequirements.AddRangeAsync(requirements);
        }
    }
} 