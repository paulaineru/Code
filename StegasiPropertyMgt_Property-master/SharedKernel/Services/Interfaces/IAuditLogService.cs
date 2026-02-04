// SharedKernel/Services/IAuditLogService.cs
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SharedKernel.Models;

namespace SharedKernel.Services
{
    public interface IAuditLogService
    {
        Task RecordActionAsync(string action, Guid? entityId, Guid? userId, string details, Guid moduleId);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(Guid? entityId = null, Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}