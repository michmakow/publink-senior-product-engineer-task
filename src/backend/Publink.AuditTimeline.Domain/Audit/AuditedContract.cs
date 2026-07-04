namespace Publink.AuditTimeline.Domain.Audit;

public sealed class AuditedContract
{
    private AuditedContract()
    {
    }

    public AuditedContract(
        string contractId,
        string contractNumber,
        DateTimeOffset createdAt,
        string createdBy)
    {
        ContractId = GuardRequired(contractId, nameof(contractId));
        ContractNumber = GuardRequired(contractNumber, nameof(contractNumber));
        CreatedAt = createdAt;
        CreatedBy = GuardRequired(createdBy, nameof(createdBy));
    }

    public string ContractId { get; private set; } = string.Empty;

    public string ContractNumber { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;

    private static string GuardRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
