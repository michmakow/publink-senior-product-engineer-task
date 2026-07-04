using Publink.AuditTimeline.Domain.Audit;

namespace Publink.AuditTimeline.Application.Mappings;

public static class AuditLabelMapper
{
    private static readonly IReadOnlyDictionary<AuditEntityType, string> EntityLabels =
        new Dictionary<AuditEntityType, string>
        {
            [AuditEntityType.Unknown] = "Nieznany obiekt",
            [AuditEntityType.ContractHeaderEntity] = "Umowa",
            [AuditEntityType.AnnexHeaderEntity] = "Aneks",
            [AuditEntityType.AnnexChangeEntity] = "Zmiana aneksu",
            [AuditEntityType.FileEntity] = "Plik",
            [AuditEntityType.InvoiceEntity] = "Faktura",
            [AuditEntityType.PaymentScheduleEntity] = "Harmonogram płatności",
            [AuditEntityType.ContractFundingEntity] = "Finansowanie"
        };

    private static readonly IReadOnlyDictionary<AuditChangeType, string> ChangeTypeLabels =
        new Dictionary<AuditChangeType, string>
        {
            [AuditChangeType.Added] = "Dodano",
            [AuditChangeType.Deleted] = "Usunięto",
            [AuditChangeType.Modified] = "Zmieniono"
        };

    private static readonly IReadOnlyDictionary<string, string> FieldLabels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Amount"] = "Kwota",
            ["AnnexNumber"] = "Numer aneksu",
            ["ContractValue"] = "Wartość umowy",
            ["ContractorName"] = "Kontrahent",
            ["DueDate"] = "Termin płatności",
            ["FileName"] = "Nazwa pliku",
            ["FundingSource"] = "Źródło finansowania",
            ["InvoiceNumber"] = "Numer faktury",
            ["Status"] = "Status"
        };

    public static string GetEntityLabel(AuditEntityType entityType)
    {
        return EntityLabels.TryGetValue(entityType, out var label)
            ? label
            : EntityLabels[AuditEntityType.Unknown];
    }

    public static string GetChangeTypeLabel(AuditChangeType changeType)
    {
        return ChangeTypeLabels.TryGetValue(changeType, out var label)
            ? label
            : changeType.ToString();
    }

    public static string GetFieldLabel(string fieldName)
    {
        return FieldLabels.TryGetValue(fieldName, out var label)
            ? label
            : fieldName;
    }

    public static string BuildDescription(
        AuditLogEntry entry,
        string entityLabel,
        string fieldLabel)
    {
        return entry.ChangeType switch
        {
            AuditChangeType.Added => $"Dodano {entityLabel.ToLowerInvariant()}: {fieldLabel} = {FormatValue(entry.NewValue)}",
            AuditChangeType.Deleted => $"Usunięto {entityLabel.ToLowerInvariant()}: {fieldLabel} = {FormatValue(entry.OldValue)}",
            AuditChangeType.Modified => $"Zmieniono {fieldLabel.ToLowerInvariant()} z {FormatValue(entry.OldValue)} na {FormatValue(entry.NewValue)}",
            _ => $"{GetChangeTypeLabel(entry.ChangeType)} {entityLabel.ToLowerInvariant()}"
        };
    }

    private static string FormatValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "brak wartości" : value;
    }
}
