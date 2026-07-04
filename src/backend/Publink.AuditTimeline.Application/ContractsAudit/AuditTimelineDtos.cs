namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed record AuditTimelineResponse(
    string ContractId,
    AuditSummaryDto Summary,
    int ReturnedItems,
    bool HasMoreItems,
    IReadOnlyList<AuditTimelineItemDto> Items);

public sealed record ContractAuditSearchResponse(
    int TotalContracts,
    int ReturnedContracts,
    bool HasMoreContracts,
    IReadOnlyList<ContractAuditSearchItemDto> Contracts);

public sealed record ContractAuditSearchItemDto(
    string ContractId,
    string ContractNumber,
    AuditSummaryDto Summary);

public sealed record AuditSummaryDto(
    int TotalChanges,
    int AddedCount,
    int ModifiedCount,
    int DeletedCount,
    int UsersInvolved,
    DateTimeOffset? FirstChangeAt,
    DateTimeOffset? LastChangeAt);

public sealed record AuditTimelineItemDto(
    string Id,
    DateTimeOffset ChangedAt,
    string ChangedBy,
    string EntityType,
    string EntityLabel,
    string ChangeType,
    string ChangeTypeLabel,
    string FieldName,
    string FieldLabel,
    string? OldValue,
    string? NewValue,
    string Description,
    bool IsInitialState = false);
