# ADR-0005: Modular Boundary vs Microservice

## Status

Accepted for MVP: modular boundary inside application.

---

## Context

Audit może w przyszłości stać się osobnym platformowym capability, ale MVP ma jedno źródło danych i jeden główny use case.

---

## Options considered

### Option A: Modular boundary

Zalety:

- czytelna separacja odpowiedzialności,
- mały koszt,
- łatwa ewolucja,
- brak kosztów infrastrukturalnych.

Wady:

- brak pełnej niezależności deploymentu.

### Option B: Microservice

Zalety:

- niezależny lifecycle,
- lepsza izolacja,
- możliwość skalowania osobno.

Wady:

- większy koszt operacyjny,
- dodatkowa infrastruktura,
- większa złożoność,
- niepotrzebne w MVP.

---

## Decision

W MVP tworzę wyraźną granicę modułu audit w kodzie, ale nie wydzielam mikroserwisu.

---

## Business value

Dostarczam funkcję szybciej i bez kosztów infrastrukturalnych, jednocześnie zostawiając ścieżkę ewolucji.

---

## When to revisit

Mikroserwis rozważyłbym, gdy:

- audit ma wielu konsumentów,
- wiele zespołów rozwija go niezależnie,
- wymaga osobnego skalowania,
- wymaga niezależnego deploymentu,
- pojawia się wiele źródeł audytu.
