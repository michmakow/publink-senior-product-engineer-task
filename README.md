# Audit Timeline MVP

## Kontekst

Skarbnik otrzymał zapowiedź kontroli RIO i potrzebuje szybko odpowiedzieć na pytanie:

> Kto, kiedy i co zmienił na umowach?

Dostępne źródło danych: istniejący `AuditLog`.

Celem rozwiązania nie jest zbudowanie kompletnej platformy audytowej, tylko dostarczenie małego MVP, które pomaga skarbnikowi przygotować się do kontroli.

\---

## Główna decyzja produktowa

W MVP buduję **Audit Timeline**: prosty widok historii zmian dla wskazanej umowy.

Nie buduję pełnego Event Sourcingu, GraphQL, mikroserwisu ani LLM, ponieważ te elementy nie są wymagane, aby zweryfikować główną hipotezę:

> Czy skarbnik potrafi znaleźć odpowiedź „kto, kiedy i co zmienił?” w mniej niż minutę?

\---

## Uruchomienie aplikacji

Najprostsza ścieżka uruchomienia:

```powershell
docker compose -f infrastructure/docker-compose.yml up --build
```

Po starcie kontenerów:

- frontend: http://localhost:5173
- backend health check: http://localhost:5080/health
- SQL Server: `localhost,1433`

Endpoint audytu wymaga nagłówka `X-Audit-Api-Key`. W MVP jest to tylko demo gate, ponieważ klucz wysyłany przez frontend nie jest sekretem. Domyślna wartość dla środowiska lokalnego to `local-dev-audit-key`.

Przykład:

```powershell
curl.exe -H "X-Audit-Api-Key: local-dev-audit-key" http://localhost:5080/api/contracts/123/audit
```

Opcjonalnie można skopiować `infrastructure/.env.example` do własnego pliku `.env` i ustawić `MSSQL_SA_PASSWORD` oraz `AUDIT_API_KEY`.

---

## Dane przykładowe

Backend seeduje dane przy starcie przez EF Core:

- `123` albo `UM-2026-001` - umowa z kilkoma zmianami w harmonogramie, finansowaniu, fakturze i plikach.
- `456` albo `UM-2026-002` - druga aktywna umowa, przydatna do sprawdzenia wyników filtrowania po wielu umowach.
- `789` albo `UM-2026-003` - umowa z aneksem, zmianą aneksu, plikiem i harmonogramem.
- `321` albo `UM-2026-004` - umowa z dodaną i usuniętą fakturą oraz korektą wartości.
- `654` albo `UM-2026-005` - umowa z finansowaniem, harmonogramem i usuniętym plikiem.
- `987` albo `UM-2025-099` - starsza umowa przechodząca przez granicę 2025/2026.
- `222` albo `UM-2026-006` - umowa z aneksem, zmianą wartości, plikiem i fakturą.
- `333` albo `UM-2026-007` - umowa z późniejszymi zmianami, usuniętym finansowaniem i wpisem legacy.
- `NO-CHANGES` albo `UM-2026-000` - umowa bez zmian, pokazuje stan pierwotny.
- `UNKNOWN` albo `UM-2026-404` - wpis z nieznanym typem encji, sprawdza fallback etykiet.

---

## Aktualny widok UI

Frontend pokazuje jeden roboczy ekran:

- pole wyszukiwania numeru albo ID umowy,
- filtry po dacie, typie zmiany, typie obiektu i użytkowniku,
- listę znalezionych umów, gdy użyto filtrów,
- suwak zakresu aktywności tylko wtedy, gdy filtry zwróciły więcej niż jedną umowę,
- klikalne karty wyników, które po rozwinięciu pokazują timeline i pełne szczegóły wybranej umowy,
- poziomy timeline z ikonami zdarzeń,
- tooltip po hoverze albo focusie na punkcie timeline,
- kartę aktywnego zdarzenia z datą, użytkownikiem, akcją, obiektem, polem oraz zmianą wartości,
- szczegółową sumaryzację z licznikami oraz listą użytkowników i akcji, których dokonali,
- komunikaty błędów po polsku, np. gdy backend nie odpowiada.

UI nie pokazuje badge'a z kluczem API. Nagłówek `X-Audit-Api-Key` pozostaje tylko lokalnym mechanizmem zabezpieczenia endpointu.

---

## Architektura aplikacji

```text
src/
  backend/
    Publink.AuditTimeline.Api
    Publink.AuditTimeline.Application
    Publink.AuditTimeline.Domain
    Publink.AuditTimeline.Infrastructure
  frontend/
    React + Vite + TypeScript
infrastructure/
  docker-compose.yml
  backend.Dockerfile
  frontend.Dockerfile
```

Backend jest modularnym monolitem: API obsługuje HTTP i autoryzację, Application przypadek użycia i DTO, Domain model audytu, Infrastructure EF Core i SQL.

---

## Zabezpieczenia endpointu w MVP

W próbce nie buduję pełnego RBAC. Endpoint nie jest anonimowy, ale użyte zabezpieczenie nie zastępuje produkcyjnej autoryzacji:

- `GET /api/contracts/{contractId}/audit` i `GET /api/contracts/audit-search` wymagają nagłówka `X-Audit-Api-Key`.
- Klucz z frontendu jest traktowany tylko jako demo gate.
- CORS jest ograniczony do originów frontendu.
- CORS dopuszcza tylko metodę `GET` i nagłówek `X-Audit-Api-Key`.
- Filtry są walidowane po stronie API.
- EF Core używa parametryzowanych zapytań LINQ.
- Wynik jest limitowany do 200 wpisów.
- Endpoint audytu ma prosty limit 60 requestów na minutę.
- API nie zwraca surowych wyjątków aplikacji.

To jest świadomy kompromis MVP. W produkcji API key z frontendu trzeba zastąpić realnym logowaniem, np. OIDC/JWT, politykami dostępu do konkretnych umów i audit access logiem.

SQL Server w Docker Compose jest wystawiony wyłącznie na `127.0.0.1:1433`; domyślne hasło, `sa`, `Encrypt=False` i `TrustServerCertificate=True` są ustawieniami lokalnymi, nie produkcyjnymi.

---

## Jak czytać dokumentację

Dokumentacja jest ułożona tak, aby pokazać tok myślenia od produktu do architektury:

1. [Executive Summary](docs/00-executive-summary.md)
2. [Problem Discovery](docs/01-problem-discovery.md)
3. [User Story \& Journey](docs/02-user-story-and-journey.md)
4. [Opportunity Solution Tree](docs/03-opportunity-solution-tree.md)
5. [MVP Definition](docs/04-mvp-definition.md)
6. [Success Metrics](docs/05-success-metrics.md)
7. [Solution Approach](docs/06-solution-approach.md)
8. [API Contract](docs/07-api-contract.md)
9. [UI Concept](docs/08-ui-concept.md)
10. [C4 Model](docs/09-c4-model.md)
11. [Event Storming](docs/10-event-storming.md)
12. [Domain Model](docs/11-domain-model.md)
13. [Architecture Roadmap](docs/12-architecture-roadmap.md)
14. [Distributed Consistency](docs/13-distributed-consistency.md)
15. [Risk Analysis](docs/14-risk-analysis.md)
16. [MVP Validation Plan](docs/15-mvp-validation-plan.md)
17. [Future AI Vision](docs/16-future-ai-vision.md)
18. [Delivery Plan](docs/17-delivery-plan.md)
19. [What I Did Not Build](docs/18-what-i-did-not-build.md)
20. [MVP Delivery Breakdown](docs/19-mvp-delivery-breakdown.md)

ADR-y:

* [ADR-0001: REST vs GraphQL](docs/adr/ADR-0001-rest-vs-graphql.md)
* [ADR-0002: Existing AuditLog vs Event Sourcing](docs/adr/ADR-0002-existing-auditlog-vs-event-sourcing.md)
* [ADR-0003: Simple Read API vs CQRS](docs/adr/ADR-0003-simple-read-api-vs-cqrs.md)
* [ADR-0004: Deterministic Summary vs LLM](docs/adr/ADR-0004-deterministic-summary-vs-llm.md)
* [ADR-0005: Modular Boundary vs Microservice](docs/adr/ADR-0005-modular-boundary-vs-microservice.md)

\---

## MVP scope

### W zakresie

* REST API nad istniejącym `AuditLog`.
* React UI z wyszukiwaniem, wynikami umów, warunkowym suwakiem i widokiem timeline.
* Filtrowanie po dacie, typie zmiany, typie encji i użytkowniku po stronie API oraz w UI.
* Mapowanie technicznych nazw encji na język użytkownika.
* Deterministyczne podsumowanie historii zmian.
* EF Core + SQL Server z seedem danych MVP.
* Docker Compose w folderze `infrastructure`.
* Dokumentacja decyzji i kompromisów.

### Poza zakresem MVP

* Pełny Event Sourcing.
* GraphQL.
* Mikroserwis auditowy.
* LLM / AI Summary w produkcyjnym sensie.
* Eksport PDF / Excel.
* Pełny system uprawnień.
* Zaawansowane wyszukiwanie pełnotekstowe.
* Cross-module audit dla Podatków i Dotacji.

\---

## Najważniejsza zasada

Każda decyzja jest uzasadniona wartością dla skarbnika lub biznesu, a nie elegancją techniczną.
