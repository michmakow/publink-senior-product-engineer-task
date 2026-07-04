namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed class SearchContractAuditHandler
{
    public const int MaxReturnedContracts = 50;
    private const int MaxContractSearchLength = 64;
    private const int MaxUserFilterLength = 128;
    private readonly IAuditLogRepository _repository;

    public SearchContractAuditHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContractAuditSearchResponse> HandleAsync(
        SearchContractAuditQuery query,
        CancellationToken cancellationToken)
    {
        Validate(query);

        var result = await _repository.SearchContractsAsync(
            NormalizeContractSearch(query.ContractIdOrNumber),
            NormalizeFilters(query.Filters),
            MaxReturnedContracts,
            cancellationToken);

        var contracts = result.Contracts
            .Select(match => new ContractAuditSearchItemDto(
                match.Contract.ContractId,
                match.Contract.ContractNumber,
                BuildSummary(match.Summary)))
            .ToArray();

        return new ContractAuditSearchResponse(
            result.TotalContracts,
            contracts.Length,
            result.TotalContracts > contracts.Length,
            contracts);
    }

    private static string? NormalizeContractSearch(string? contractIdOrNumber)
    {
        return string.IsNullOrWhiteSpace(contractIdOrNumber) ? null : contractIdOrNumber.Trim();
    }

    private static ContractAuditFilters NormalizeFilters(ContractAuditFilters filters)
    {
        return filters with
        {
            User = string.IsNullOrWhiteSpace(filters.User) ? null : filters.User.Trim()
        };
    }

    private static AuditSummaryDto BuildSummary(AuditLogQuerySummary summary)
    {
        return new AuditSummaryDto(
            summary.TotalChanges,
            summary.AddedCount,
            summary.ModifiedCount,
            summary.DeletedCount,
            summary.UsersInvolved,
            summary.FirstChangeAt,
            summary.LastChangeAt);
    }

    private static void Validate(SearchContractAuditQuery query)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (query.ContractIdOrNumber?.Length > MaxContractSearchLength)
        {
            errors["contractId"] = [$"Identyfikator umowy nie może przekraczać {MaxContractSearchLength} znaków."];
        }

        if (query.Filters.From.HasValue && query.Filters.To.HasValue && query.Filters.From > query.Filters.To)
        {
            errors["dateRange"] = ["Data od nie może być późniejsza niż data do."];
        }

        if (query.Filters.User?.Length > MaxUserFilterLength)
        {
            errors["user"] = [$"Filtr użytkownika nie może przekraczać {MaxUserFilterLength} znaków."];
        }

        if (errors.Count > 0)
        {
            throw new ContractAuditValidationException(errors);
        }
    }
}
