using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Auditing;

public sealed class AuditLog : Entity
{
    private AuditLog() { }
    public AuditLog(Guid? userId, string action, string entityType, string entityId, string? changesJson, string correlationId)
    {
        UserId = userId; Action = action; EntityType = entityType; EntityId = entityId;
        ChangesJson = changesJson; CorrelationId = correlationId;
    }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;
    public string? ChangesJson { get; private set; }
    public string CorrelationId { get; private set; } = null!;
    public string? IpAddress { get; private set; }
    public string? Workstation { get; private set; }
}
