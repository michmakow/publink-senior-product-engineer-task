namespace Publink.AuditTimeline.Application.ContractsAudit;

public sealed class ContractAuditValidationException : Exception
{
    public ContractAuditValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Contract audit query is invalid.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
