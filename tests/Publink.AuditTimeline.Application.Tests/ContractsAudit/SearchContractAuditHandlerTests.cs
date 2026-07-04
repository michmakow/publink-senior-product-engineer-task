using Publink.AuditTimeline.Application.ContractsAudit;
using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Application.Tests.ContractsAudit;

public sealed class SearchContractAuditHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsContractsMatchingFiltersNewestFirst()
    {
        var repository = new FakeAuditLogRepository();
        repository.Contracts.AddRange(
        [
            Contract("123", "UM-2026-001"),
            Contract("456", "UM-2026-002"),
            Contract("789", "UM-2026-003")
        ]);
        repository.Entries.AddRange(
        [
            Entry("audit-1", "123", AuditChangeType.Modified, AtUtc(2026, 5, 1), "anna.nowak"),
            Entry("audit-2", "456", AuditChangeType.Modified, AtUtc(2026, 6, 1), "anna.nowak"),
            Entry("audit-3", "789", AuditChangeType.Added, AtUtc(2026, 7, 1), "anna.nowak")
        ]);

        var result = await CreateHandler(repository).HandleAsync(
            new SearchContractAuditQuery(null, EmptyFilters() with { ChangeType = AuditChangeType.Modified }),
            CancellationToken.None);

        Assert.Equal(2, result.TotalContracts);
        Assert.Equal(2, result.ReturnedContracts);
        Assert.False(result.HasMoreContracts);
        Assert.Equal(["456", "123"], result.Contracts.Select(contract => contract.ContractId));
        Assert.All(result.Contracts, contract => Assert.Equal(1, contract.Summary.ModifiedCount));
    }

    [Fact]
    public async Task HandleAsync_TrimsSearchAndUserFiltersBeforeRepositoryCall()
    {
        var repository = new FakeAuditLogRepository();

        await CreateHandler(repository).HandleAsync(
            new SearchContractAuditQuery("  UM-2026  ", EmptyFilters() with { User = "  anna  " }),
            CancellationToken.None);

        Assert.Equal("UM-2026", repository.LastContractIdOrNumber);
        Assert.Equal("anna", repository.LastFilters?.User);
    }

    [Fact]
    public async Task HandleAsync_ReturnsHasMoreWhenRepositoryFindsMoreThanLimit()
    {
        var repository = new FakeAuditLogRepository
        {
            ForcedTotalContracts = SearchContractAuditHandler.MaxReturnedContracts + 1
        };

        for (var index = 0; index < SearchContractAuditHandler.MaxReturnedContracts; index++)
        {
            repository.SearchMatches.Add(new ContractAuditSearchMatch(
                Contract(index.ToString("D3"), $"UM-2026-{index:D3}"),
                new AuditLogQuerySummary(1, 0, 1, 0, 1, AtUtc(2026, 1, 1), AtUtc(2026, 1, 1))));
        }

        var result = await CreateHandler(repository).HandleAsync(
            new SearchContractAuditQuery(null, EmptyFilters()),
            CancellationToken.None);

        Assert.Equal(SearchContractAuditHandler.MaxReturnedContracts, result.ReturnedContracts);
        Assert.True(result.HasMoreContracts);
    }

    [Fact]
    public async Task HandleAsync_ThrowsValidationExceptionForInvalidDateRange()
    {
        var exception = await Assert.ThrowsAsync<ContractAuditValidationException>(
            () => CreateHandler(new FakeAuditLogRepository()).HandleAsync(
                new SearchContractAuditQuery(
                    null,
                    EmptyFilters() with { From = AtUtc(2026, 5, 2), To = AtUtc(2026, 5, 1) }),
                CancellationToken.None));

        Assert.Contains("dateRange", exception.Errors.Keys);
    }

    private static SearchContractAuditHandler CreateHandler(FakeAuditLogRepository repository)
    {
        return new SearchContractAuditHandler(repository);
    }

    private static ContractAuditFilters EmptyFilters()
    {
        return new ContractAuditFilters(null, null, null, null, null);
    }

    private static AuditedContract Contract(string contractId, string contractNumber)
    {
        return new AuditedContract(contractId, contractNumber, AtUtc(2026, 1, 1), "system");
    }

    private static AuditLogEntry Entry(
        string id,
        string contractId,
        AuditChangeType changeType,
        DateTimeOffset changedAt,
        string changedBy)
    {
        return new AuditLogEntry(
            id,
            contractId,
            $"entity-{id}",
            AuditEntityType.PaymentScheduleEntity,
            changeType,
            changedAt,
            changedBy,
            "DueDate",
            "2026-07-01",
            "2026-07-15");
    }

    private static DateTimeOffset AtUtc(int year, int month, int day)
    {
        return new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
    }

    private sealed class FakeAuditLogRepository : IAuditLogRepository
    {
        public List<AuditedContract> Contracts { get; } = [];

        public List<AuditLogEntry> Entries { get; } = [];

        public List<ContractAuditSearchMatch> SearchMatches { get; } = [];

        public int? ForcedTotalContracts { get; init; }

        public string? LastContractIdOrNumber { get; private set; }

        public ContractAuditFilters? LastFilters { get; private set; }

        public Task<AuditedContract?> FindContractAsync(
            string contractIdOrNumber,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<AuditLogQueryResult> GetEntriesAsync(
            string contractId,
            ContractAuditFilters filters,
            int limit,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<ContractAuditSearchResult> SearchContractsAsync(
            string? contractIdOrNumber,
            ContractAuditFilters filters,
            int limit,
            CancellationToken cancellationToken)
        {
            LastContractIdOrNumber = contractIdOrNumber;
            LastFilters = filters;

            if (SearchMatches.Count > 0 || ForcedTotalContracts.HasValue)
            {
                return Task.FromResult(new ContractAuditSearchResult(
                    SearchMatches.Take(limit).ToArray(),
                    ForcedTotalContracts ?? SearchMatches.Count));
            }

            var query = Entries.AsEnumerable();

            if (filters.ChangeType.HasValue)
            {
                query = query.Where(entry => entry.ChangeType == filters.ChangeType.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.User))
            {
                query = query.Where(entry =>
                    entry.ChangedBy.Contains(filters.User, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(contractIdOrNumber))
            {
                query = query.Where(entry =>
                    Contracts.Any(contract =>
                        contract.ContractId == entry.ContractId
                        && (contract.ContractId.Contains(contractIdOrNumber, StringComparison.OrdinalIgnoreCase)
                            || contract.ContractNumber.Contains(contractIdOrNumber, StringComparison.OrdinalIgnoreCase))));
            }

            var matches = query
                .GroupBy(entry => entry.ContractId)
                .Select(group =>
                {
                    var contract = Contracts.Single(item => item.ContractId == group.Key);
                    var entries = group.ToArray();

                    return new ContractAuditSearchMatch(
                        contract,
                        new AuditLogQuerySummary(
                            entries.Length,
                            entries.Count(entry => entry.ChangeType == AuditChangeType.Added),
                            entries.Count(entry => entry.ChangeType == AuditChangeType.Modified),
                            entries.Count(entry => entry.ChangeType == AuditChangeType.Deleted),
                            entries.Select(entry => entry.ChangedBy).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                            entries.Min(entry => entry.ChangedAt),
                            entries.Max(entry => entry.ChangedAt)));
                })
                .OrderByDescending(match => match.Summary.LastChangeAt)
                .Take(limit)
                .ToArray();

            return Task.FromResult(new ContractAuditSearchResult(matches, matches.Length));
        }

        public Task<bool> HasAnyEntriesAsync(
            string contractId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
