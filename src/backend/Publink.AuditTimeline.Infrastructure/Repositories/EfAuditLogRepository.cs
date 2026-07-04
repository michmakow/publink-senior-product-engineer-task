using Microsoft.EntityFrameworkCore;
using Publink.AuditTimeline.Application.ContractsAudit;
using Publink.AuditTimeline.Domain.Audit;
using Publink.AuditTimeline.Infrastructure.Persistence;

namespace Publink.AuditTimeline.Infrastructure.Repositories;

public sealed class EfAuditLogRepository : IAuditLogRepository
{
    private readonly AuditDbContext _dbContext;

    public EfAuditLogRepository(AuditDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditedContract?> FindContractAsync(
        string contractIdOrNumber,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Contracts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                contract => contract.ContractId == contractIdOrNumber
                    || contract.ContractNumber == contractIdOrNumber,
                cancellationToken);
    }

    public async Task<AuditLogQueryResult> GetEntriesAsync(
        string contractId,
        ContractAuditFilters filters,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogEntries
            .AsNoTracking()
            .Where(entry => entry.ContractId == contractId);

        if (filters.From.HasValue)
        {
            query = query.Where(entry => entry.ChangedAt >= filters.From.Value);
        }

        if (filters.To.HasValue)
        {
            query = query.Where(entry => entry.ChangedAt <= filters.To.Value);
        }

        if (filters.ChangeType.HasValue)
        {
            query = query.Where(entry => entry.ChangeType == filters.ChangeType.Value);
        }

        if (filters.EntityType.HasValue)
        {
            query = query.Where(entry => entry.EntityType == filters.EntityType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.User))
        {
            query = query.Where(entry => entry.ChangedBy.Contains(filters.User));
        }

        var totalChanges = await query.CountAsync(cancellationToken);
        var summary = totalChanges == 0
            ? new AuditLogQuerySummary(0, 0, 0, 0, 0, null, null)
            : new AuditLogQuerySummary(
                totalChanges,
                await query.CountAsync(entry => entry.ChangeType == AuditChangeType.Added, cancellationToken),
                await query.CountAsync(entry => entry.ChangeType == AuditChangeType.Modified, cancellationToken),
                await query.CountAsync(entry => entry.ChangeType == AuditChangeType.Deleted, cancellationToken),
                await query.Select(entry => entry.ChangedBy).Distinct().CountAsync(cancellationToken),
                await query.MinAsync(entry => entry.ChangedAt, cancellationToken),
                await query.MaxAsync(entry => entry.ChangedAt, cancellationToken));

        var entries = await query
            .OrderByDescending(entry => entry.ChangedAt)
            .ThenBy(entry => entry.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new AuditLogQueryResult(entries, summary);
    }

    public async Task<ContractAuditSearchResult> SearchContractsAsync(
        string? contractIdOrNumber,
        ContractAuditFilters filters,
        int limit,
        CancellationToken cancellationToken)
    {
        var query =
            from entry in _dbContext.AuditLogEntries.AsNoTracking()
            join contract in _dbContext.Contracts.AsNoTracking()
                on entry.ContractId equals contract.ContractId
            select new
            {
                Contract = contract,
                Entry = entry
            };

        if (!string.IsNullOrWhiteSpace(contractIdOrNumber))
        {
            query = query.Where(row =>
                row.Contract.ContractId.Contains(contractIdOrNumber)
                || row.Contract.ContractNumber.Contains(contractIdOrNumber));
        }

        if (filters.From.HasValue)
        {
            query = query.Where(row => row.Entry.ChangedAt >= filters.From.Value);
        }

        if (filters.To.HasValue)
        {
            query = query.Where(row => row.Entry.ChangedAt <= filters.To.Value);
        }

        if (filters.ChangeType.HasValue)
        {
            query = query.Where(row => row.Entry.ChangeType == filters.ChangeType.Value);
        }

        if (filters.EntityType.HasValue)
        {
            query = query.Where(row => row.Entry.EntityType == filters.EntityType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.User))
        {
            query = query.Where(row => row.Entry.ChangedBy.Contains(filters.User));
        }

        var groupedQuery = query
            .GroupBy(row => new
            {
                row.Contract.ContractId,
                row.Contract.ContractNumber,
                row.Contract.CreatedAt,
                row.Contract.CreatedBy
            })
            .Select(group => new
            {
                group.Key.ContractId,
                group.Key.ContractNumber,
                group.Key.CreatedAt,
                group.Key.CreatedBy,
                TotalChanges = group.Count(),
                AddedCount = group.Count(row => row.Entry.ChangeType == AuditChangeType.Added),
                ModifiedCount = group.Count(row => row.Entry.ChangeType == AuditChangeType.Modified),
                DeletedCount = group.Count(row => row.Entry.ChangeType == AuditChangeType.Deleted),
                UsersInvolved = group.Select(row => row.Entry.ChangedBy).Distinct().Count(),
                FirstChangeAt = group.Min(row => row.Entry.ChangedAt),
                LastChangeAt = group.Max(row => row.Entry.ChangedAt)
            });

        var totalContracts = await groupedQuery.CountAsync(cancellationToken);

        var rows = await groupedQuery
            .OrderByDescending(row => row.LastChangeAt)
            .ThenBy(row => row.ContractNumber)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var contracts = rows
            .Select(row => new ContractAuditSearchMatch(
                new AuditedContract(
                    row.ContractId,
                    row.ContractNumber,
                    row.CreatedAt,
                    row.CreatedBy),
                new AuditLogQuerySummary(
                    row.TotalChanges,
                    row.AddedCount,
                    row.ModifiedCount,
                    row.DeletedCount,
                    row.UsersInvolved,
                    row.FirstChangeAt,
                    row.LastChangeAt)))
            .ToArray();

        return new ContractAuditSearchResult(contracts, totalContracts);
    }

    public async Task<bool> HasAnyEntriesAsync(
        string contractId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AuditLogEntries
            .AsNoTracking()
            .AnyAsync(entry => entry.ContractId == contractId, cancellationToken);
    }
}
