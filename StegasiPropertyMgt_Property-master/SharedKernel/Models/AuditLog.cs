using System;

namespace SharedKernel.Models;

public class AuditLog
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string Details { get; set; } = string.Empty;
    public Guid ModuleId { get; set; }
    public DateTime Timestamp { get; set; }
}


