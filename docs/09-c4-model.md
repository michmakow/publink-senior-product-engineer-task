# 09. C4 Model

## C1 - System Context

```mermaid
flowchart TD
    Treasurer[Skarbnik] --> AuditSystem[Audit Timeline MVP]
    AuditSystem --> ExistingDb[(Existing SQL AuditLog)]
    AuditSystem --> ContractSystem[System Umów]

    RIO[Kontroler RIO] -. otrzymuje wyjaśnienia .-> Treasurer
```

### Opis

Skarbnik korzysta z Audit Timeline MVP, aby odczytać historię zmian na umowie. Źródłem danych jest istniejący `AuditLog`.

---

## C2 - Container Diagram

```mermaid
flowchart TD
    Browser[Browser / React SPA] --> Api[.NET Audit API]
    Api --> Db[(SQL Database: AuditLog)]

    subgraph Audit Timeline MVP
      Browser
      Api
    end
```

### Kontenery

| Kontener | Odpowiedzialność |
|---|---|
| React SPA | Prezentacja timeline, filtrów i summary |
| .NET API | Pobieranie i mapowanie danych audytowych |
| SQL AuditLog | Istniejące źródło danych |

---

## C3 - Component Diagram dla API

```mermaid
flowchart TD
    Controller[AuditController] --> QueryHandler[GetContractAuditQueryHandler]
    QueryHandler --> Repository[AuditLogRepository]
    QueryHandler --> Mapper[AuditTimelineMapper]
    QueryHandler --> SummaryBuilder[AuditSummaryBuilder]
    Repository --> Db[(SQL AuditLog)]
    Mapper --> Dto[AuditTimelineResponse]
    SummaryBuilder --> Dto
```

### Komponenty

| Komponent | Odpowiedzialność |
|---|---|
| `AuditController` | HTTP contract |
| `GetContractAuditQueryHandler` | przypadek użycia |
| `AuditLogRepository` | odczyt z bazy |
| `AuditTimelineMapper` | mapowanie techniczne -> biznesowe |
| `AuditSummaryBuilder` | budowanie podsumowania |

---

## Przyszły C2 - event-driven audit

```mermaid
flowchart LR
    Contracts[Umowy] --> Outbox1[Outbox]
    Taxes[Podatki] --> Outbox2[Outbox]
    Grants[Dotacje] --> Outbox3[Outbox]

    Outbox1 --> Bus[Azure Service Bus]
    Outbox2 --> Bus
    Outbox3 --> Bus

    Bus --> Ingestion[Audit Ingestion Worker]
    Ingestion --> Projection[(Audit Projection Store)]
    Projection --> QueryApi[Audit Query API]
    QueryApi --> Ui[Audit UI]
```

---

## Dlaczego nie buduję tego teraz?

Obecnie MVP ma jedno źródło danych. Wydzielenie event-driven platformy teraz zwiększyłoby złożoność bez proporcjonalnej wartości dla skarbnika.

[Previous](08-ui-concept.md) | [Next](10-event-storming.md)
