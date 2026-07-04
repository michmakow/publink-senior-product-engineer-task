# 03. Opportunity Solution Tree

## Cel biznesowy

Skrócić czas przygotowania odpowiedzi dla RIO dotyczącej historii zmian na umowie.

---

## Opportunity Solution Tree

```mermaid
flowchart TD
    O[Outcome: skarbnik odpowiada na pytanie RIO w mniej niż minutę]

    O --> OP1[Opportunity: trudność w odczytaniu technicznych logów]
    O --> OP2[Opportunity: brak czytelnej sekwencji zmian]
    O --> OP3[Opportunity: zależność od IT]
    O --> OP4[Opportunity: ryzyko pominięcia ważnej zmiany]

    OP1 --> S1[Solution: mapowanie encji na język biznesowy]
    OP1 --> S2[Solution: prosty field diff]

    OP2 --> S3[Solution: timeline chronologiczny]
    OP2 --> S4[Solution: grupowanie po operacji biznesowej w przyszłości]

    OP3 --> S5[Solution: self-service widok dla skarbnika]
    OP3 --> S6[Solution: filtry bez zapytań SQL]

    OP4 --> S7[Solution: summary zmian]
    OP4 --> S8[Solution: future AI assistant]
```

---

## Wybrane rozwiązania do MVP

| Problem | Rozwiązanie MVP | Dlaczego teraz |
|---|---|---|
| Techniczne nazwy encji | Mapowanie na nazwy biznesowe | Szybka poprawa zrozumiałości |
| Brak narracji | Timeline | Lepiej odpowiada na pytanie „co wydarzyło się po kolei?” |
| Zbyt wiele danych | Filtry | Skarbnik może ograniczyć zakres kontroli |
| Brak szybkiego obrazu sytuacji | Deterministyczne summary | Ułatwia orientację przed wejściem w szczegóły |

---

## Rozwiązania odłożone na później

| Pomysł | Dlaczego nie w MVP |
|---|---|
| AI Summary | Najpierw potrzebujemy wiarygodnych i deterministycznych danych |
| Export PDF | Przydatny produkcyjnie, ale niepotrzebny do walidacji hipotezy |
| Cross-module audit | Nie ma jeszcze wielu modułów jako źródeł |
| GraphQL | Use case jest zbyt wąski |
| Event Sourcing | Koszt wdrożenia większy niż wartość dla próbki MVP |

---

## Najważniejsza decyzja

MVP optymalizuje **czas dojścia do odpowiedzi**, nie kompletność platformy.

[Previous](02-user-story-and-journey.md) | [Next](04-mvp-definition.md)
