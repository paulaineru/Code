using System;

namespace AuthenticationService.Configuration
{
    public class RoleConfiguration
    {
        public Guid AdminRoleId { get; set; }
        public Guid UserRoleId { get; set; }
        public Guid TenantRoleId { get; set; }
        public Guid PropertyManagerRoleId { get; set; }
        public Guid EstatesOfficerRoleId { get; set; }
        public Guid MaintenanceOfficerRoleId { get; set; }
        public Guid FinanceTeamRoleId { get; set; }
        public Guid SalesOfficerRoleId { get; set; }
        public Guid SalesManagerRoleId { get; set; }
    }
} 