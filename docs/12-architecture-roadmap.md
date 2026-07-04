# 12. Architecture Roadmap

## Cel

Pokazać, jak rozwiązanie może ewoluować od prostego MVP do skalowalnej platformy audytowej, bez budowania tej złożoności za wcześnie.

---

## Etap 1 - MVP

```mermaid
flowchart LR
    React[React UI] --> Api[.NET REST API]
    Api --> Sql[(SQL AuditLog)]
```

### Charakterystyka

- jedno źródło danych,
- jeden główny przypadek użycia,
- szybkie dostarczenie wartości,
- minimum infrastruktury.

---

## Etap 2 - Production Hardening

```mermaid
flowchart LR
    React[React UI] --> Api[.NET REST API]
    Api --> Sql[(SQL AuditLog)]
    Api --> AppInsights[Application Insights]
    Api --> Auth[Authorization]
```

### Zakres

- autoryzacja,
- monitoring,
- logging strukturalny,
- dashboard,
- podstawowe SLO,
- testy integracyjne,
- obsługa błędów.

---

## Etap 3 - Audit Read Model

```mermaid
flowchart LR
    Source[(SQL AuditLog)] --> Projection[Audit Projection Builder]
    Projection --> ReadModel[(Audit Read Model)]
    ReadModel --> Api[Audit Query API]
    Api --> UI[Audit UI]
```

### Kiedy?

Gdy zapytania do AuditLog są wolne lub UI potrzebuje modelu bardziej dopasowanego do odczytu.

---

## Etap 4 - Event-driven Audit

```mermaid
flowchart LR
    Contracts[Umowy] --> O1[Outbox]
    Taxes[Podatki] --> O2[Outbox]
    Grants[Dotacje] --> O3[Outbox]

    O1 --> Bus[Azure Service Bus]
    O2 --> Bus
    O3 --> Bus

    Bus --> Ingestion[Audit Ingestion]
    Ingestion --> Projection[(Audit Projection Store)]
    Projection --> Api[Audit Query API]
    Api --> UI[Audit UI]
```

### Kiedy?

Gdy pojawią się moduły Podatki i Dotacje oraz audit będzie pochodził z wielu źródeł.

---

## Etap 5 - Audit Platform

```mermaid
flowchart TD
    AuditPlatform[Audit Platform]
    AuditPlatform --> Timeline[Timeline]
    AuditPlatform --> Search[Cross-module Search]
    AuditPlatform --> Export[Control Package Export]
    AuditPlatform --> AI[AI Audit Assistant]
    AuditPlatform --> Alerts[Risk Signals]
```

### Charakterystyka

- wspólna platforma audytu,
- wiele modułów,
- zaawansowane wyszukiwanie,
- integracja AI,
- raporty dla kontroli,
- analiza ryzyk.

---

## Kiedy uruchamiam zmianę?

Nie teraz. Zmianę uruchamiam dopiero, gdy pojawi się realny trigger:

```mermaid
flowchart TD
    A[Czy jest drugie źródło audytu?] -->|Nie| B[Zostań przy MVP]
    A -->|Tak| C[Czy obecne API blokuje rozwój?]
    C -->|Nie| D[Dodaj adapter / read model]
    C -->|Tak| E[Uruchom event-driven audit]
```

---

## Najważniejsza zasada

Architektura powinna rosnąć razem z produktem, nie przed produktem.

[Previous](11-domain-model.md) | [Next](13-distributed-consistency.md)
