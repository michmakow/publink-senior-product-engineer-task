# ADR-0003: Simple Read API vs CQRS

## Status

Accepted for MVP: simple read API.

---

## Context

MVP nie posiada command side. Nie zapisuje zmian w umowach. Tylko odczytuje historię.

---

## Options considered

### Option A: Simple query API

Zalety:

- mało kodu,
- prosty przepływ,
- szybka realizacja,
- pasuje do read-only MVP.

Wady:

- ograniczona skalowalność przy bardziej złożonych scenariuszach.

### Option B: Full CQRS

Zalety:

- separacja read/write,
- dobre przy rozbudowanym modelu odczytu,
- łatwiejsza optymalizacja query side.

Wady:

- w MVP nie ma command side,
- większa złożoność,
- ryzyko overengineeringu.

---

## Decision

W MVP implementuję prosty read API.

Model odpowiedzi jest jednak projektowany jako read model pod UI.

---

## Business value

Szybciej dostarczam wartość skarbnikowi i nie buduję abstrakcji, które nie są potrzebne do walidacji MVP.

---

## When to revisit

CQRS rozważyłbym, gdy:

- audit stanie się osobnym capability,
- pojawią się ciężkie zapytania,
- powstanie osobny read model,
- różne UI będą miały różne wymagania odczytowe.
