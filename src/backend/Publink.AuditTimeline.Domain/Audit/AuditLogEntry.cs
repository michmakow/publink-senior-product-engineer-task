namespace Publink.AuditTimeline.Domain.Audit;

public sealed class AuditLogEntry
{
    private AuditLogEntry()
    {
    }

    public AuditLogEntry(
        string id,
        string contractId,
        string entityId,
        AuditEntityType entityType,
        AuditChangeType changeType,
        DateTimeOffset changedAt,
        string changedBy,
        string fieldName,
        string? oldValue,
        string? newValue)
    {
        Id = GuardRequired(id, nameof(id));
        ContractId = GuardRequired(contractId, nameof(contractId));
        EntityId = GuardRequired(entityId, nameof(entityId));
        EntityType = entityType;
        ChangeType = changeType;
        ChangedAt = changedAt;
        ChangedBy = GuardRequired(changedBy, nameof(changedBy));
        FieldName = GuardRequired(fieldName, nameof(fieldName));
        OldValue = NormalizeOptional(oldValue);
        NewValue = NormalizeOptional(newValue);
    }

    public string Id { get; private set; } = string.Empty;

    public string ContractId { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public AuditEntityType EntityType { get; private set; }

    public AuditChangeType ChangeType { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public string ChangedBy { get; private set; } = string.Empty;

    public string FieldName { get; private set; } = string.Empty;

    public string? OldValue { get; private set; }

    public string? NewValue { get; private set; }

    private static string GuardRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
