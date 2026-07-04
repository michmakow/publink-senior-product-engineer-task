namespace Publink.AuditTimeline.Domain.Audit;

public enum AuditEntityType
{
    Unknown = 0,
    ContractHeaderEntity = 1,
    AnnexHeaderEntity = 2,
    AnnexChangeEntity = 3,
    FileEntity = 4,
    InvoiceEntity = 5,
    PaymentScheduleEntity = 6,
    ContractFundingEntity = 7
}
