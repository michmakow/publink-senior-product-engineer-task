namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed record GetContractAuditQuery(
    string ContractId,
    ContractAuditFilters Filters);
