using SharedKernel.Models;
using AuthenticationService.Repository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Services
{
    public interface IRoleAuditService
    {
        Task LogRoleActionAsync(Guid roleId, string action, string performedBy, object? oldValues = null, object? newValues = null, Guid? affectedUserId = null, string? notes = null);
        Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogsAsync(Guid roleId, DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<RoleAuditLog>> GetUserRoleAuditLogsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    }

    public class RoleAuditService : IRoleAuditService
    {
        private readonly IAuthDbContext _context;
        private readonly ILogger<RoleAuditService> _logger;

        public RoleAuditService(IAuthDbContext context, ILogger<RoleAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogRoleActionAsync(Guid roleId, string action, string performedBy, object? oldValues = null, object? newValues = null, Guid? affectedUserId = null, string? notes = null)
        {
            try
            {
                var log = new RoleAuditLog
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    Action = action,
                    PerformedBy = performedBy,
                    PerformedAt = DateTime.UtcNow,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    AffectedUserId = affectedUserId,
                    Notes = notes
                };

                _context.RoleAuditLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Logged role action: {Action} for role {RoleId} by user {PerformedBy}", 
                    action, roleId, performedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging role action for role {RoleId}", roleId);
                throw;
            }
        }

        public async Task<IEnumerable<RoleAuditLog>> GetRoleAuditLogsAsync(Guid roleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.RoleAuditLogs
                .Where(log => log.RoleId == roleId);

            if (startDate.HasValue)
            {
                query = query.Where(log => log.PerformedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.PerformedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.PerformedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoleAuditLog>> GetUserRoleAuditLogsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.RoleAuditLogs
                .Where(log => log.AffectedUserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(log => log.PerformedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(log => log.PerformedAt <= endDate.Value);
            }

            return await query
                .OrderByDescending(log => log.PerformedAt)
                .ToListAsync();
        }
    }
} 