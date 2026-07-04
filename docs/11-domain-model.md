# 11. Domain Model

## Cel modelu domenowego

Model domenowy MVP jest celowo mały. Nie próbuję modelować całego obszaru umów. Modeluję tylko to, co jest potrzebne do odpowiedzi na pytanie RIO.

---

## Kluczowe pojęcia

```mermaid
classDiagram
    class ContractAuditTimeline {
      string ContractId
      AuditSummary Summary
      AuditTimelineItem[] Items
    }

    class AuditTimelineItem {
      string Id
      datetime ChangedAt
      string ChangedBy
      ChangeType ChangeType
      AuditedEntity Entity
      AuditFieldChange[] FieldChanges
    }

    class AuditedEntity {
      EntityType Type
      string Label
      string EntityId
    }

    class AuditFieldChange {
      string FieldName
      string FieldLabel
      string OldValue
      string NewValue
    }

    class AuditSummary {
      int TotalChanges
      int AddedCount
      int ModifiedCount
      int DeletedCount
      int UsersInvolved
    }

    ContractAuditTimeline --> AuditSummary
    ContractAuditTimeline --> AuditTimelineItem
    AuditTimelineItem --> AuditedEntity
    AuditTimelineItem --> AuditFieldChange
```

---

## Enumy z zadania

### Type

```csharp
public enum Type
{
    Added = 1,
    Deleted = 2,
    Modified = 3,
}
```

### EntityType

```csharp
public enum EntityType
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
```

---

## Mapowanie na język użytkownika

```mermaid
flowchart LR
    A[ContractHeaderEntity] --> A1[Umowa]
    B[AnnexHeaderEntity] --> B1[Aneks]
    C[AnnexChangeEntity] --> C1[Zmiana aneksu]
    D[FileEntity] --> D1[Plik]
    E[InvoiceEntity] --> E1[Faktura]
    F[PaymentScheduleEntity] --> F1[Harmonogram płatności]
    G[ContractFundingEntity] --> G1[Finansowanie]
```

---

## Dlaczego model odpowiedzi nie kopiuje tabeli AuditLog?

Ponieważ UI powinno być zależne od potrzeb użytkownika, nie od struktury bazy.

To daje dwie korzyści:

1. Skarbnik dostaje czytelny model.
2. W przyszłości można podmienić źródło danych bez przepisywania UI.

[Previous](10-event-storming.md) | [Next](12-architecture-roadmap.md)
