// SharedKernel/Models/Tenant.cs
using System;
using System.Collections.Generic;
using SharedKernel.Models.Tenants;

namespace SharedKernel.Models
{
    public class Tenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string PrimaryEmail { get; set; }
        public string PrimaryTelephone { get; set; }
        public string TaxIdentificationNumber { get; set; }
        public string? BusinessRegistrationNumber { get; set; }
        public  NotificationPreferences NotificationPreferences{ get; set; } = NotificationPreferences.Email; 
        public Status Status { get; set; } = Status.Active;
        public TenantType TenantType { get; set; } = TenantType.CorporateOrganisation;
        public List<ContactDetail> Contacts { get; set; } = new();
        public string BillingEntity { get; set; }
        public List<Booking> Bookings { get; set; } = new();
        public List<RenewalRequest> RenewalRequests { get; set; } = new();
        public List<TerminationProcess> TerminationProcesses { get; set; } = new();
        public virtual List<LeaseAgreement> LeaseAgreements { get; set; } = new();
    }

    public enum TenantType
    {
        Individual,
        CorporateOrganisation,
        GovernmentAgency
    }
    public enum NotificationPreferences
    {
        Email,
        SMS
    }
    public enum Status
    {
        Active,
        Inactive,
        Suspended,
        Terminated
    }

}