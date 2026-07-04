# Implementation Report

## Cel

Ten plik podsumowuje pliki utworzone lub zmienione podczas implementacji MVP oraz wiąże je z zadaniami z delivery breakdown.

## Backend API, EF i SQL

| Plik | Zakres |
|---|---|
| `Publink.AuditTimeline.sln` | Rozwiązanie .NET łączące projekty backendowe. |
| `src/backend/Publink.AuditTimeline.Api/Program.cs` | Konfiguracja API, CORS, API key auth, health check, DI i inicjalizacja bazy. |
| `src/backend/Publink.AuditTimeline.Api/Controllers/ContractsAuditController.cs` | Endpointy `GET /api/contracts/{contractId}/audit` i `GET /api/contracts/audit-search`, walidacja query params z polskimi komunikatami, rate limiting i statusy 400/401/404/200. |
| `src/backend/Publink.AuditTimeline.Api/Security/ApiKeyAuthenticationHandler.cs` | Proste zabezpieczenie endpointu nagłówkiem `X-Audit-Api-Key`. |
| `src/backend/Publink.AuditTimeline.Api/appsettings.json` | Lokalny connection string, API key i dozwolone originy CORS. |
| `src/backend/Publink.AuditTimeline.Api/Publink.AuditTimeline.Api.csproj` | Projekt ASP.NET Core API i referencje do warstw Application/Infrastructure. |
| `src/backend/Publink.AuditTimeline.Application/ContractsAudit/*` | DTO odpowiedzi szczegółów i wyszukiwania, filtry, handlery use case, kontrakt repozytorium, pełne summary przefiltrowanego wyniku, limity `hasMoreItems`/`hasMoreContracts` i walidacja. |
| `src/backend/Publink.AuditTimeline.Application/Mappings/AuditLabelMapper.cs` | Mapowanie nazw technicznych na etykiety biznesowe oraz deterministyczne opisy zmian. |
| `src/backend/Publink.AuditTimeline.Application/Publink.AuditTimeline.Application.csproj` | Projekt warstwy Application z referencją do Domain. |
| `src/backend/Publink.AuditTimeline.Domain/Audit/*` | Mały model domenowy audytu: typ zmiany, typ encji, umowa i wpis audit log. |
| `src/backend/Publink.AuditTimeline.Domain/Publink.AuditTimeline.Domain.csproj` | Projekt warstwy Domain bez zależności infrastrukturalnych. |
| `src/backend/Publink.AuditTimeline.Infrastructure/Persistence/AuditDbContext.cs` | EF Core DbContext, mapping tabel `Contracts` i `AuditLog`, indeksy pod filtry. |
| `src/backend/Publink.AuditTimeline.Infrastructure/Persistence/AuditDbInitializer.cs` | Idempotentna inicjalizacja bazy oraz rozszerzony seed danych dla umów `123`, `456`, `789`, `321`, `654`, `987`, `222`, `333`, `444`, `555`, `666`, `777`, `888`, `NO-CHANGES` i `UNKNOWN`. |
| `src/backend/Publink.AuditTimeline.Infrastructure/Persistence/ServiceCollectionExtensions.cs` | Rejestracja EF Core, repozytorium i inicjalizatora bazy. |
| `src/backend/Publink.AuditTimeline.Infrastructure/Repositories/EfAuditLogRepository.cs` | Odczyt AuditLog przez EF Core, filtrowanie szczegółów, wyszukiwanie umów po przefiltrowanym audycie i limity wyników sterowane przez handlery. |
| `src/backend/Publink.AuditTimeline.Infrastructure/Publink.AuditTimeline.Infrastructure.csproj` | Projekt infrastruktury z zależnością `Microsoft.EntityFrameworkCore.SqlServer`. |
| `tests/Publink.AuditTimeline.Application.Tests/ContractsAudit/GetContractAuditHandlerTests.cs` | Testy krytycznej logiki MVP: summary, sortowanie, stan pierwotny, puste wyniki po filtrach, brak kontraktu i walidacje. |
| `tests/Publink.AuditTimeline.Application.Tests/ContractsAudit/SearchContractAuditHandlerTests.cs` | Testy wyszukiwania umów po filtrach: sortowanie, normalizacja, limit wyników i walidacja zakresu dat. |
| `tests/Publink.AuditTimeline.Application.Tests/Publink.AuditTimeline.Application.Tests.csproj` | Projekt testów xUnit dla warstwy Application. |

## Frontend React

| Plik | Zakres |
|---|---|
| `src/frontend/package.json` | Konfiguracja React/Vite/TypeScript i skrypty `dev`, `build`, `preview`. |
| `src/frontend/package-lock.json` | Zablokowane wersje zależności npm. |
| `src/frontend/index.html` | Punkt wejścia SPA. |
| `src/frontend/vite.config.ts` | Konfiguracja Vite. |
| `src/frontend/tsconfig.json` | Referencja do konfiguracji TypeScript aplikacji. |
| `src/frontend/tsconfig.app.json` | Ścisłe ustawienia TypeScript dla frontendu. |
| `src/frontend/src/vite-env.d.ts` | Typy `import.meta.env` dla Vite. |
| `src/frontend/src/main.tsx` | Renderowanie aplikacji React. |
| `src/frontend/src/App.tsx` | Główny widok MVP: wyszukiwarka, filtry, tryb szczegółów jednej umowy, tryb wyników po filtrach, rozwijane karty i szczegółowe summary. |
| `src/frontend/src/App.css` | Style layoutu aplikacji, nagłówka oraz stanów loading/error. |
| `src/frontend/src/styles.css` | Globalna baza stylów: `:root`, `body`, `box-sizing` i font formularzy. |
| `src/frontend/src/api/auditTimelineApi.ts` | Klient HTTP do endpointu audytu z nagłówkiem API key i mapowaniem błędów sieciowych na polski komunikat. |
| `src/frontend/src/features/auditTimeline/types.ts` | Typy odpowiedzi API, filtrów i elementów timeline. |
| `src/frontend/src/features/auditTimeline/format.ts` | Formatowanie dat dla UI. |
| `src/frontend/src/features/auditTimeline/auditText.ts` | Wspólne teksty akcji używane przez tooltipy, kartę zdarzenia i summary. |
| `src/frontend/src/features/auditTimeline/hooks/useAuditTimeline.ts` | Obsługa loading/error/not found, pobierania szczegółów timeline, czyszczenia starych danych i ignorowania spóźnionych odpowiedzi. |
| `src/frontend/src/features/auditTimeline/hooks/useAuditTimelineDetails.ts` | Cache i loading/error per umowa dla wielu równocześnie rozwiniętych kart wyników. |
| `src/frontend/src/features/auditTimeline/hooks/useContractAuditSearch.ts` | Obsługa wyszukiwania umów po filtrach, loading/error i ignorowania spóźnionych odpowiedzi. |
| `src/frontend/src/features/auditTimeline/components/ContractSearch.tsx` | Input ID/numeru umowy, filtry audytu i akcje wyszukiwania/resetu. |
| `src/frontend/src/features/auditTimeline/components/ContractSearch.css` | Style formularza wyszukiwania umowy i panelu filtrów. |
| `src/frontend/src/features/auditTimeline/components/ContractSearchResults.tsx` | Lista znalezionych umów, warunkowy suwak zakresu aktywności i wiele równocześnie rozwijanych kart z timeline oraz summary. |
| `src/frontend/src/features/auditTimeline/components/ContractSearchResults.css` | Style listy wyników, kart, wyróżnionych nagłówków otwartych umów, suwaka i stanów inline. |
| `src/frontend/src/features/auditTimeline/components/EventCarousel.tsx` | Pozioma linia czasu z ikonami zdarzeń, tooltipami, klikanym wyborem aktywnego zdarzenia, strzałkami i kartą danych zdarzenia; przy jednej akcji nie renderuje osi timeline. |
| `src/frontend/src/features/auditTimeline/components/EventCarousel.css` | Style timeline, tooltipów, strzałek i karty aktywnego zdarzenia. |
| `src/frontend/src/features/auditTimeline/components/DetailedAuditSummary.tsx` | Szczegółowa sumaryzacja całej historii: użytkownicy z akcjami, liczniki, modyfikacje, dodania i usunięcia. |
| `src/frontend/src/features/auditTimeline/components/DetailedAuditSummary.css` | Style summary, liczników, list użytkowników i list zmian. |

## Docker i uruchomienie

| Plik | Zakres |
|---|---|
| `infrastructure/docker-compose.yml` | Uruchamia SQL Server, backend i frontend jednym poleceniem; SQL jest zbindowany do `127.0.0.1`. |
| `infrastructure/backend.Dockerfile` | Build i runtime ASP.NET Core API; restore dotyczy projektu API, aby nie wymagać kopiowania testów w obrazie. |
| `infrastructure/frontend.Dockerfile` | Build React/Vite i serwowanie przez Nginx. |
| `infrastructure/nginx.conf` | Fallback SPA do `index.html` oraz nagłówki cache: `index.html` bez cache, haszowane assety jako immutable. |
| `infrastructure/.env.example` | Przykładowe zmienne dla hasła SQL i API key. |
| `.dockerignore` | Pomija artefakty builda i zależności przy budowie obrazów. |
| `.gitignore` | Pomija `bin`, `obj`, `node_modules`, `dist` i lokalne sekrety. |

## Dokumentacja

| Plik | Zakres |
|---|---|
| `README.md` | Dodana instrukcja uruchomienia, dane seed, architektura i zabezpieczenia endpointu. |
| `docs/07-api-contract.md` | Uzupełniony kontrakt o endpoint szczegółów, endpoint wyszukiwania, `returnedItems`, `hasMoreItems` i `hasMoreContracts`. |
| `docs/18-what-i-did-not-build.md` | Poprawiony link do delivery breakdown. |
| `docs/implementation-report.md` | Ten raport zmian. |

## Powiązanie z delivery breakdown

| Zadanie | Realizacja |
|---|---|
| T1 Contract Audit Search | `ContractSearch`, `useContractAuditSearch`, `useAuditTimelineDetails`, `ContractSearchResults`, obsługa loading/error/not found, filtrów i wielu otwartych kart. |
| T2 Contract Audit Endpoint | `ContractsAuditController`, `GetContractAuditHandler`, `SearchContractAuditHandler`, DTO response. |
| T3 Business Labels Mapping | `AuditLabelMapper`, UI pokazuje etykiety biznesowe z API. |
| T4 Audit Timeline | `EventCarousel.tsx` pokazuje poziomą linię czasu z ikonami zdarzeń dla umowy. |
| T5 Event Navigation and Tooltips | `EventCarousel.tsx` zapewnia klikane punkty timeline, tooltipy i przejście poprzednie/następne. |
| T6 Initial State Marker | `BuildInitialStateResponse`, seed `NO-CHANGES`; stan pierwotny pojawia się jako zdarzenie na osi. |
| T7 Deterministic Summary | `BuildSummary`, `DetailedAuditSummary.tsx`. |
| T8 README and Decision Notes | `README.md`, `docs/implementation-report.md`. |
| Minimal tests | `GetContractAuditHandlerTests` i `SearchContractAuditHandlerTests` pokrywają logikę największego ryzyka bez zależności od SQL/Dockera. |

## Weryfikacja wykonana lokalnie

- `dotnet build Publink.AuditTimeline.sln` - sukces.
- `dotnet test Publink.AuditTimeline.sln --no-restore` - sukces, 14/14 testów.
- `npm run build` w `src/frontend` - sukces.
- `docker compose -f infrastructure/docker-compose.yml up --build -d` - sukces.
- `curl.exe http://localhost:5080/health` - sukces, API zwraca `{"status":"ok"}`.
- `curl.exe -H "X-Audit-Api-Key: local-dev-audit-key" "http://localhost:5080/api/contracts/audit-search"` - sukces, API zwraca 14 umów z historią audytu.
- `curl.exe -H "X-Audit-Api-Key: local-dev-audit-key" "http://localhost:5080/api/contracts/audit-search?changeType=Deleted"` - sukces, API zwraca 7 umów z usunięciami.
- `curl.exe -H "X-Audit-Api-Key: local-dev-audit-key" "http://localhost:5080/api/contracts/333/audit"` - sukces, API zwraca timeline z 6 wpisami.
- `curl.exe -H "X-Audit-Api-Key: local-dev-audit-key" "http://localhost:5080/api/contracts/444/audit"` - sukces, API zwraca timeline z 7 wpisami.
- `curl.exe -H "X-Audit-Api-Key: local-dev-audit-key" "http://localhost:5080/api/contracts/777/audit"` - sukces, API zwraca timeline z 9 wpisami.
- `curl.exe http://localhost:5173` - sukces, frontend serwuje aktualny bundle `index-C9B6072X.js`.
- Błąd walidacji API dla niepoprawnej daty zwraca polski komunikat: `Data musi mieć format yyyy-MM-dd albo poprawny format ISO.`
