# 18. What I Did Not Build

## Cel

Pokazać świadome odpuszczenie zakresu.

W MVP równie ważne jak to, co buduję, jest to, czego nie buduję.

---

## Nie buduję GraphQL

### Dlaczego?

MVP ma jeden główny przypadek użycia: pobranie historii zmian dla umowy.

REST jest prostszy, szybszy i bardziej czytelny.

### Kiedy wróciłbym do GraphQL?

Gdy pojawi się wiele widoków:

- historia umowy,
- historia użytkownika,
- historia modułu,
- dashboard kontroli,
- cross-module audit.

---

## Nie buduję pełnego Event Sourcingu

### Dlaczego?

Skarbnik potrzebuje audytu, nie rekonstrukcji całego stanu systemu.

Event Sourcing w MVP zwiększyłby koszt i ryzyko bez proporcjonalnej wartości.

### Kiedy wróciłbym do Event Sourcingu lub event-driven audit?

Gdy audit będzie pochodził z wielu modułów i stanie się platformowym capability.

---

## Nie buduję mikroserwisu

### Dlaczego?

W MVP nie ma jeszcze niezależnego cyklu życia ani wielu źródeł danych.

Wydzielenie mikroserwisu teraz byłoby decyzją infrastrukturalną, nie produktową.

---

## Nie integruję LLM

### Dlaczego?

W kontroli RIO najważniejsza jest wiarygodność.

Najpierw buduję deterministyczny timeline i summary. LLM może być kolejnym krokiem, ale z kontrolą jakości i źródłami.

---

## Nie buduję eksportu PDF / Excel

### Dlaczego?

Eksport jest prawdopodobnie przydatny produkcyjnie, ale nie jest konieczny do walidacji głównej hipotezy.

Jeżeli użytkownicy potwierdzą wartość timeline, eksport byłby jednym z pierwszych kolejnych kroków.

---

## Nie buduję pełnych uprawnień

### Dlaczego?

To jest zadanie rekrutacyjne i próbka MVP.

W produkcji auth/RBAC jest krytyczne, ale w próbce nie zwiększa wartości oceny głównego toku myślenia.

---

## Decision Matrix

| Element | Teraz | Później | Powód |
|---|---|---|---|
| REST | Tak | Tak | Najprostszy kontrakt MVP |
| GraphQL | Nie | Możliwe | Potrzebne dopiero przy wielu perspektywach |
| SQL AuditLog | Tak | Możliwe jako źródło | Obecne źródło danych |
| Event-driven audit | Nie | Tak | Przy wielu modułach |
| LLM | Nie | Tak | Najpierw wiarygodne dane |
| Export | Nie | Tak | Po potwierdzeniu wartości |
| Microservice | Nie | Możliwe | Gdy audit ma niezależny lifecycle |

---

## Najważniejszy wniosek

Nie buduję mniej dlatego, że czegoś nie umiem.

Buduję mniej, ponieważ MVP powinno walidować najważniejszą hipotezę możliwie najkrótszą drogą.

[Previous](17-delivery-plan.md) | [Next](19-mvp-delivery-breakdown.md)
