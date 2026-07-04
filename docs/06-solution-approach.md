# 06. Solution Approach

## Podejście

W MVP wybieram prostą architekturę:

```mermaid
flowchart LR
    UI[React UI] --> API[.NET REST API]
    API --> DB[(SQL AuditLog)]
```

To rozwiązanie jest wystarczające, aby zbudować pierwszy użyteczny widok i zweryfikować hipotezę MVP.

---

## Główne komponenty

### React UI

Odpowiada za:

- formularz wyszukiwania umowy,
- timeline,
- wybór zdarzenia na osi czasu,
- tooltipy i kartę szczegółów zdarzenia,
- summary z akcjami użytkowników,
- empty/error states.

### .NET API

Odpowiada za:

- pobranie danych z AuditLog,
- mapowanie danych technicznych na DTO dla UI,
- filtrowanie,
- sortowanie,
- przygotowanie summary.

### SQL AuditLog

W MVP traktowany jako istniejące źródło prawdy dla historii zmian.

---

## Flow danych

```mermaid
sequenceDiagram
    actor Treasurer as Skarbnik
    participant UI as React UI
    participant API as .NET API
    participant DB as SQL AuditLog

    Treasurer->>UI: Wpisuje ID umowy
    UI->>API: GET /api/contracts/{id}/audit
    API->>DB: Query AuditLog
    DB-->>API: Rekordy auditowe
    API->>API: Mapowanie i summary
    API-->>UI: Timeline DTO
    UI-->>Treasurer: Czytelna historia zmian
```

---

## Dlaczego REST?

REST lepiej pasuje do MVP, bo przypadek użycia jest prosty i dobrze zdefiniowany.

GraphQL zostaje świadomie odłożony na przyszłość, jeśli pojawią się różne perspektywy audytu i bardziej elastyczne zapytania.

---

## Dlaczego nie pełny Event Sourcing teraz?

AuditLog odpowiada na pytanie:

> kto, kiedy i co zmienił?

Event Sourcing odpowiada na szersze pytanie:

> czy cały stan systemu można odtworzyć ze zdarzeń?

W MVP skarbnik potrzebuje audytu, a nie pełnej rekonstrukcji stanu domeny.

---

## Warstwa antykorupcyjna

Nawet w MVP nie chcę, żeby UI zależało od technicznych szczegółów tabeli AuditLog.

Dlatego API zwraca model biznesowy:

```mermaid
flowchart LR
    A[Raw AuditLog] --> B[Audit Mapping]
    B --> C[Business Timeline DTO]
    C --> D[React UI]
```

To pozwala w przyszłości podmienić SQL AuditLog na event-driven projection bez przepisywania frontendu.

[Previous](05-success-metrics.md) | [Next](07-api-contract.md)
