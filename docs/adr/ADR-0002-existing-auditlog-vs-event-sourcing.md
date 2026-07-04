# ADR-0002: Existing AuditLog vs Event Sourcing

## Status

Accepted for MVP: use existing AuditLog.

---

## Context

Zadanie mówi: „Masz AuditLog”.

MVP ma pomóc skarbnikowi zobaczyć historię zmian na umowach.

---

## Options considered

### Option A: Existing AuditLog

Zalety:

- zgodne z treścią zadania,
- szybka implementacja,
- wystarczające do odpowiedzi „kto, kiedy i co zmienił”.

Wady:

- zależność od jakości istniejącego loga,
- ograniczenia przy wielu modułach.

### Option B: Full Event Sourcing

Zalety:

- pełna historia domenowa,
- możliwość rekonstrukcji stanu,
- silny model zdarzeniowy.

Wady:

- duży koszt implementacji,
- zmiana paradygmatu systemu,
- niepotrzebne do walidacji MVP.

---

## Decision

W MVP używam istniejącego AuditLoga i mapuję go na timeline zdarzeń biznesowych.

Nie implementuję pełnego Event Sourcingu.

---

## Business value

Skarbnik potrzebuje szybkiej odpowiedzi przed kontrolą RIO. Existing AuditLog pozwala dostarczyć wartość szybciej.

---

## When to revisit

Event-driven audit lub Event Sourcing rozważyłbym, gdy:

- pojawią się moduły Podatki i Dotacje,
- audit będzie pochodził z wielu źródeł,
- historia zmian stanie się podstawowym modelem prawdy,
- pojawi się potrzeba odtwarzania stanu domeny ze zdarzeń.
