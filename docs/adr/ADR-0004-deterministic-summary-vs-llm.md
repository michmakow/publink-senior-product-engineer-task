# ADR-0004: Deterministic Summary vs LLM

## Status

Accepted for MVP: deterministic summary.

---

## Context

AI jest atrakcyjnym kierunkiem dla audit assistant, ale w kontroli RIO kluczowa jest wiarygodność.

---

## Options considered

### Option A: Deterministic summary

Zalety:

- przewidywalne,
- łatwe do przetestowania,
- brak halucynacji,
- proste do weryfikacji.

Wady:

- mniej elastyczne,
- mniej „inteligentne”.

### Option B: LLM summary

Zalety:

- naturalny język,
- możliwość wyjaśniania złożonych zmian,
- potencjalnie duża wartość dla użytkownika.

Wady:

- ryzyko halucynacji,
- potrzeba ewaluacji jakości,
- konieczność zabezpieczeń,
- większy koszt MVP.

---

## Decision

W MVP używam deterministic summary.

---

## Business value

Skarbnik otrzymuje wiarygodne podsumowanie bez ryzyka, że AI dopowie coś, czego nie ma w danych.

---

## When to revisit

LLM dodałbym, gdy:

- mamy stabilny structured timeline,
- mamy testy jakości promptów,
- każde twierdzenie AI można powiązać ze źródłowym audit entry,
- użytkownik zawsze widzi dane źródłowe.
