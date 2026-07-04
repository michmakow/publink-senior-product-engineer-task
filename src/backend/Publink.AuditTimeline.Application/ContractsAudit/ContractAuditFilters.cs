using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed record ContractAuditFilters(
    DateTimeOffset? From,
    DateTimeOffset? To,
    AuditChangeType? ChangeType,
    AuditEntityType? EntityType,
    string? User);
