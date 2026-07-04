namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed record SearchContractAuditQuery(
    string? ContractIdOrNumber,
    ContractAuditFilters Filters);
