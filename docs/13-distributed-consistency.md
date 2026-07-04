# 13. Distributed Consistency

## Kontekst

Zadanie wspomina, że za 6 miesięcy portfolio modułów może rosnąć o Podatki i Dotacje, a audit przestanie pochodzić z jednej bazy SQL.

W takim świecie nie można zakładać jednej transakcji ACID obejmującej wszystkie moduły.

---

## Przykład problemu

> Aneks został zapisany, ale harmonogram płatności jeszcze nie.

To może oznaczać:

- aneks został zapisany w module Umowy,
- zdarzenie audytowe dla aneksu powstało,
- harmonogram płatności jest przetwarzany asynchronicznie,
- zdarzenie dla harmonogramu pojawi się później,
- albo trafi do obsługi błędów.

---

## Jak to pokazać w audycie?

Nie udajemy pełnej spójności.

Pokazujemy stan procesu:

```mermaid
stateDiagram-v2
    [*] --> AnnexSaved
    AnnexSaved --> SchedulePending
    SchedulePending --> ScheduleSaved
    SchedulePending --> ScheduleFailed
    ScheduleFailed --> RetryScheduled
    RetryScheduled --> ScheduleSaved
```

---

## Mechanizmy

### Outbox Pattern

Zdarzenie audytowe zapisuję w tej samej lokalnej transakcji co zmiana domenowa.

```mermaid
sequenceDiagram
    participant App as Moduł Umowy
    participant DB as Local DB
    participant Outbox as Outbox Table
    participant Bus as Service Bus

    App->>DB: Save Annex
    App->>Outbox: Save Audit Event
    DB-->>App: Commit
    Outbox->>Bus: Publish event async
```

---

### Idempotency Key

Chroni przed podwójnym przetworzeniem tego samego zdarzenia.

```mermaid
flowchart LR
    E[Audit Event] --> K{Known Idempotency Key?}
    K -->|Yes| Ignore[Ignore duplicate]
    K -->|No| Store[Store event]
```

---

### Correlation ID

Łączy wiele technicznych zmian w jedną operację biznesową.

```mermaid
flowchart TD
    C[Correlation ID: update-contract-123] --> A[Annex Modified]
    C --> B[Payment Schedule Changed]
    C --> D[File Added]
```

---

### Dead Letter Queue

Nie gubimy zdarzeń, których nie udało się przetworzyć.

```mermaid
flowchart LR
    Event[Audit Event] --> Processor[Ingestion Processor]
    Processor -->|Success| Store[Projection Store]
    Processor -->|Failure after retries| DLQ[Dead Letter Queue]
```

---

## Co robię w MVP?

W MVP nie implementuję tych mechanizmów.

Opisuję je jako docelowy kierunek, ponieważ obecnie mamy jedno źródło danych i jedno zapytanie odczytowe.

---

## Najważniejsza decyzja

W systemie rozproszonym audit powinien pokazywać prawdę o stanie procesu, nawet jeśli jest on częściowy.

Nie powinien ukrywać niespójności, bo w kontekście kontroli ważniejsza jest wiarygodność niż ładna narracja.

[Previous](12-architecture-roadmap.md) | [Next](14-risk-analysis.md)
