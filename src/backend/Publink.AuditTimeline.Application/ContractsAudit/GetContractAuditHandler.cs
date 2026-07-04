using Publink.AuditTimeline.Application.Mappings;
using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed class GetContractAuditHandler
{
    public const int MaxReturnedItems = 200;
    private const int MaxContractIdLength = 64;
    private const int MaxUserFilterLength = 128;
    private readonly IAuditLogRepository _repository;

    public GetContractAuditHandler(IAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<AuditTimelineResponse?> HandleAsync(
        GetContractAuditQuery query,
        CancellationToken cancellationToken)
    {
        Validate(query);

        var normalizedContractKey = query.ContractId.Trim();
        var contract = await _repository.FindContractAsync(normalizedContractKey, cancellationToken);
        if (contract is null)
        {
            return null;
        }

        var auditLog = await _repository.GetEntriesAsync(
            contract.ContractId,
            NormalizeFilters(query.Filters),
            MaxReturnedItems,
            cancellationToken);

        var hasAnyEntries = auditLog.Summary.TotalChanges > 0
            || await _repository.HasAnyEntriesAsync(contract.ContractId, cancellationToken);

        if (!hasAnyEntries)
        {
            return BuildInitialStateResponse(contract);
        }

        var returnedEntries = auditLog.Entries
            .OrderByDescending(entry => entry.ChangedAt)
            .ThenBy(entry => entry.Id, StringComparer.Ordinal)
            .Take(MaxReturnedItems)
            .ToArray();

        return new AuditTimelineResponse(
            contract.ContractId,
            BuildSummary(auditLog.Summary),
            returnedEntries.Length,
            auditLog.Summary.TotalChanges > returnedEntries.Length,
            returnedEntries.Select(MapEntry).ToArray());
    }

    private static ContractAuditFilters NormalizeFilters(ContractAuditFilters filters)
    {
        return filters with
        {
            User = string.IsNullOrWhiteSpace(filters.User) ? null : filters.User.Trim()
        };
    }

    private static AuditTimelineItemDto MapEntry(AuditLogEntry entry)
    {
        var entityLabel = AuditLabelMapper.GetEntityLabel(entry.EntityType);
        var changeTypeLabel = AuditLabelMapper.GetChangeTypeLabel(entry.ChangeType);
        var fieldLabel = AuditLabelMapper.GetFieldLabel(entry.FieldName);

        return new AuditTimelineItemDto(
            entry.Id,
            entry.ChangedAt,
            entry.ChangedBy,
            entry.EntityType.ToString(),
            entityLabel,
            entry.ChangeType.ToString(),
            changeTypeLabel,
            entry.FieldName,
            fieldLabel,
            entry.OldValue,
            entry.NewValue,
            AuditLabelMapper.BuildDescription(entry, entityLabel, fieldLabel));
    }

    private static AuditTimelineResponse BuildInitialStateResponse(AuditedContract contract)
    {
        var summary = new AuditSummaryDto(
            TotalChanges: 0,
            AddedCount: 0,
            ModifiedCount: 0,
            DeletedCount: 0,
            UsersInvolved: 0,
            FirstChangeAt: contract.CreatedAt,
            LastChangeAt: contract.CreatedAt);

        var item = new AuditTimelineItemDto(
            Id: $"initial-{contract.ContractId}",
            ChangedAt: contract.CreatedAt,
            ChangedBy: contract.CreatedBy,
            EntityType: AuditEntityType.ContractHeaderEntity.ToString(),
            EntityLabel: AuditLabelMapper.GetEntityLabel(AuditEntityType.ContractHeaderEntity),
            ChangeType: "InitialState",
            ChangeTypeLabel: "Stan pierwotny",
            FieldName: "InitialState",
            FieldLabel: "Stan umowy",
            OldValue: null,
            NewValue: "Brak zmian po utworzeniu",
            Description: "Po utworzeniu umowy nie odnotowano zmian.",
            IsInitialState: true);

        return new AuditTimelineResponse(contract.ContractId, summary, ReturnedItems: 1, HasMoreItems: false, [item]);
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

    private static void Validate(GetContractAuditQuery query)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(query.ContractId))
        {
            errors["contractId"] = ["Podaj numer albo ID umowy."];
        }
        else if (query.ContractId.Length > MaxContractIdLength)
        {
            errors["contractId"] = [$"Identyfikator umowy nie może przekraczać {MaxContractIdLength} znaków."];
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
