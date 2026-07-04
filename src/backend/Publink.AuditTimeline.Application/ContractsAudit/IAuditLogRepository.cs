using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Application.ContractsAudit;

public interface IAuditLogRepository
{
    Task<AuditedContract?> FindContractAsync(
        string contractIdOrNumber,
        CancellationToken cancellationToken);

    Task<AuditLogQueryResult> GetEntriesAsync(
        string contractId,
        ContractAuditFilters filters,
        int limit,
        CancellationToken cancellationToken);

    Task<ContractAuditSearchResult> SearchContractsAsync(
        string? contractIdOrNumber,
        ContractAuditFilters filters,
        int limit,
        CancellationToken cancellationToken);

    Task<bool> HasAnyEntriesAsync(
        string contractId,
        CancellationToken cancellationToken);
}
