using Publink.AuditTimeline.Application.ContractsAudit;
using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Application.Tests.ContractsAudit;

public sealed class GetContractAuditHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsNewestFirstItemsAndSummary()
    {
        var repository = new FakeAuditLogRepository();
        var contract = Contract("123");
        repository.Contracts.Add(contract);
        repository.Entries.AddRange(
        [
            Entry("older", "123", AuditChangeType.Added, AuditEntityType.ContractHeaderEntity, AtUtc(2026, 1, 12), "anna.nowak"),
            Entry("newer", "123", AuditChangeType.Modified, AuditEntityType.PaymentScheduleEntity, AtUtc(2026, 6, 18), "jan.kowalski"),
            Entry("deleted", "123", AuditChangeType.Deleted, AuditEntityType.FileEntity, AtUtc(2026, 4, 2), "anna.nowak")
        ]);

        var result = await CreateHandler(repository).HandleAsync(
            new GetContractAuditQuery("123", EmptyFilters()),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("123", result.ContractId);
        Assert.Equal(3, result.ReturnedItems);
        Assert.False(result.HasMoreItems);
        Assert.Equal(["newer", "deleted", "older"], result.Items.Select(item => item.Id));
        Assert.Equal(3, result.Summary.TotalChanges);
        Assert.Equal(1, result.Summary.AddedCount);
        Assert.Equal(1, result.Summary.ModifiedCount);
        Assert.Equal(1, result.Summary.DeletedCount);
        Assert.Equal(2, result.Summary.UsersInvolved);
        Assert.Equal(AtUtc(2026, 1, 12), result.Summary.FirstChangeAt);
        Assert.Equal(AtUtc(2026, 6, 18), result.Summary.LastChangeAt);
        Assert.Equal("Harmonogram płatności", result.Items[0].EntityLabel);
        Assert.Equal("Zmieniono", result.Items[0].ChangeTypeLabel);
    }

    [Fact]
    public async Task HandleAsync_ReturnsInitialStateWhenContractHasNoEntries()
    {
        var repository = new FakeAuditLogRepository();
        repository.Contracts.Add(Contract("NO-CHANGES", createdAt: AtUtc(2026, 2, 3)));

        var result = await CreateHandler(repository).HandleAsync(
            new GetContractAuditQuery("NO-CHANGES", EmptyFilters()),
            CancellationToken.None);

        Assert.NotNull(result);
        var item = Assert.Single(result.Items);
        Assert.True(item.IsInitialState);
        Assert.Equal(1, result.ReturnedItems);
        Assert.False(result.HasMoreItems);
        Assert.Equal("Stan pierwotny", item.ChangeTypeLabel);
        Assert.Equal("Po utworzeniu umowy nie odnotowano zmian.", item.Description);
        Assert.Equal(0, result.Summary.TotalChanges);
        Assert.Equal(AtUtc(2026, 2, 3), result.Summary.FirstChangeAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyTimelineWhenFiltersExcludeExistingHistory()
    {
        var repository = new FakeAuditLogRepository();
        repository.Contracts.Add(Contract("123"));
        repository.Entries.Add(Entry("audit-1", "123", AuditChangeType.Modified, AuditEntityType.InvoiceEntity, AtUtc(2026, 3, 1), "anna.nowak"));

        var result = await CreateHandler(repository).HandleAsync(
            new GetContractAuditQuery(
                "123",
                EmptyFilters() with { From = AtUtc(2026, 5, 1), To = AtUtc(2026, 5, 2) }),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.ReturnedItems);
        Assert.False(result.HasMoreItems);
        Assert.Equal(0, result.Summary.TotalChanges);
        Assert.Null(result.Summary.FirstChangeAt);
        Assert.Null(result.Summary.LastChangeAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNullWhenContractDoesNotExist()
    {
        var result = await CreateHandler(new FakeAuditLogRepository()).HandleAsync(
            new GetContractAuditQuery("missing", EmptyFilters()),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_TrimsUserFilterBeforeRepositoryCall()
    {
        var repository = new FakeAuditLogRepository();
        repository.Contracts.Add(Contract("123"));
        repository.Entries.Add(Entry("audit-1", "123", AuditChangeType.Modified, AuditEntityType.InvoiceEntity, AtUtc(2026, 3, 1), "anna.nowak"));

        await CreateHandler(repository).HandleAsync(
            new GetContractAuditQuery("123", EmptyFilters() with { User = "  anna  " }),
            CancellationToken.None);

        Assert.Equal("anna", repository.LastFilters?.User);
    }

    [Fact]
    public async Task HandleAsync_ReturnsHasMoreWhenRepositoryReturnsMoreThanLimit()
    {
        var repository = new FakeAuditLogRepository();
        repository.Contracts.Add(Contract("123"));

        for (var index = 0; index < GetContractAuditHandler.MaxReturnedItems + 1; index++)
        {
            repository.Entries.Add(Entry(
                $"audit-{index:D3}",
                "123",
                AuditChangeType.Modified,
                AuditEntityType.PaymentScheduleEntity,
                AtUtc(2026, 1, 1).AddMinutes(index),
                "anna.nowak"));
        }

        var result = await CreateHandler(repository).HandleAsync(
            new GetContractAuditQuery("123", EmptyFilters()),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(GetContractAuditHandler.MaxReturnedItems, result.ReturnedItems);
        Assert.True(result.HasMoreItems);
        Assert.Equal(GetContractAuditHandler.MaxReturnedItems + 1, result.Summary.TotalChanges);
        Assert.Equal(GetContractAuditHandler.MaxReturnedItems, result.Items.Count);
    }

    [Theory]
    [MemberData(nameof(InvalidQueries))]
    public async Task HandleAsync_ThrowsValidationExceptionForInvalidQueries(GetContractAuditQuery query)
    {
        var exception = await Assert.ThrowsAsync<ContractAuditValidationException>(
            () => CreateHandler(new FakeAuditLogRepository()).HandleAsync(query, CancellationToken.None));

        Assert.NotEmpty(exception.Errors);
    }

    public static TheoryData<GetContractAuditQuery> InvalidQueries()
    {
        return new TheoryData<GetContractAuditQuery>
        {
            new("123", EmptyFilters() with { From = AtUtc(2026, 5, 2), To = AtUtc(2026, 5, 1) }),
            new(" ", EmptyFilters()),
            new(new string('x', 65), EmptyFilters()),
            new("123", EmptyFilters() with { User = new string('x', 129) })
        };
    }

    private static GetContractAuditHandler CreateHandler(FakeAuditLogRepository repository)
    {
        return new GetContractAuditHandler(repository);
    }

    private static ContractAuditFilters EmptyFilters()
    {
        return new ContractAuditFilters(null, null, null, null, null);
    }

    private static AuditedContract Contract(
        string contractId,
        DateTimeOffset? createdAt = null)
    {
        return new AuditedContract(
            contractId,
            $"UM-{contractId}",
            createdAt ?? AtUtc(2026, 1, 1),
            "system");
    }

    private static AuditLogEntry Entry(
        string id,
        string contractId,
        AuditChangeType changeType,
        AuditEntityType entityType,
        DateTimeOffset changedAt,
        string changedBy)
    {
        return new AuditLogEntry(
            id,
            contractId,
            $"{entityType}-{id}",
            entityType,
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

        public ContractAuditFilters? LastFilters { get; private set; }

        public Task<AuditedContract?> FindContractAsync(
            string contractIdOrNumber,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Contracts.FirstOrDefault(contract =>
                    contract.ContractId == contractIdOrNumber
                    || contract.ContractNumber == contractIdOrNumber));
        }

        public Task<AuditLogQueryResult> GetEntriesAsync(
            string contractId,
            ContractAuditFilters filters,
            int limit,
            CancellationToken cancellationToken)
        {
            LastFilters = filters;

            var query = Entries
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
                query = query.Where(entry => entry.ChangedBy.Contains(filters.User, StringComparison.OrdinalIgnoreCase));
            }

            var matchedEntries = query.ToArray();
            var summary = matchedEntries.Length == 0
                ? new AuditLogQuerySummary(0, 0, 0, 0, 0, null, null)
                : new AuditLogQuerySummary(
                    matchedEntries.Length,
                    matchedEntries.Count(entry => entry.ChangeType == AuditChangeType.Added),
                    matchedEntries.Count(entry => entry.ChangeType == AuditChangeType.Modified),
                    matchedEntries.Count(entry => entry.ChangeType == AuditChangeType.Deleted),
                    matchedEntries.Select(entry => entry.ChangedBy).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    matchedEntries.Min(entry => entry.ChangedAt),
                    matchedEntries.Max(entry => entry.ChangedAt));

            var entries = matchedEntries
                .OrderByDescending(entry => entry.ChangedAt)
                .ThenBy(entry => entry.Id, StringComparer.Ordinal)
                .Take(limit)
                .ToArray();

            return Task.FromResult(new AuditLogQueryResult(entries, summary));
        }

        public Task<ContractAuditSearchResult> SearchContractsAsync(
            string? contractIdOrNumber,
            ContractAuditFilters filters,
            int limit,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<bool> HasAnyEntriesAsync(
            string contractId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Entries.Any(entry => entry.ContractId == contractId));
        }
    }
}
