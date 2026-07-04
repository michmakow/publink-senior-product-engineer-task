using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed record AuditLogQueryResult(
    IReadOnlyList<AuditLogEntry> Entries,
    AuditLogQuerySummary Summary);

public sealed record ContractAuditSearchResult(
    IReadOnlyList<ContractAuditSearchMatch> Contracts,
    int TotalContracts);

public sealed record ContractAuditSearchMatch(
    AuditedContract Contract,
    AuditLogQuerySummary Summary);

public sealed record AuditLogQuerySummary(
    int TotalChanges,
    int AddedCount,
    int ModifiedCount,
    int DeletedCount,
    int UsersInvolved,
    DateTimeOffset? FirstChangeAt,
    DateTimeOffset? LastChangeAt);
