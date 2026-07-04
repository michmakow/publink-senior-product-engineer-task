# ADR-0001: REST vs GraphQL

## Status

Accepted for MVP.

---

## Context

MVP ma umożliwić pobranie historii zmian dla konkretnej umowy.

Główny przypadek użycia:

```http
GET /api/contracts/{contractId}/audit
```

---

## Options considered

### Option A: REST

Zalety:

- prosty kontrakt,
- szybka implementacja,
- łatwy do zrozumienia,
- wystarczający dla jednego głównego use case.

Wady:

- mniejsza elastyczność przy wielu różnych widokach.

### Option B: GraphQL

Zalety:

- elastyczne zapytania,
- dobre przy wielu perspektywach danych,
- potencjalnie mniej endpointów.

Wady:

- większy koszt MVP,
- dodatkowa złożoność,
- nie rozwiązuje lepiej obecnego problemu skarbnika.

---

## Decision

W MVP wybieram REST.

---

## Business value

REST pozwala szybciej dostarczyć narzędzie, które odpowiada na pytanie skarbnika.

Dodatkowa elastyczność GraphQL nie zwiększa wartości w pierwszym scenariuszu.

---

## When to revisit

Wróciłbym do GraphQL, jeśli pojawią się różne perspektywy audytu:

- historia umowy,
- historia użytkownika,
- historia modułu,
- dashboard kontroli,
- cross-module search.
