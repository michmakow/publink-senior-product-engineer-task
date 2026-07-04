import { Search } from "lucide-react";
import type { AuditFilters } from "../types";
import "./ContractSearch.css";

interface ContractSearchProps {
  contractId: string;
  filters: AuditFilters;
  isLoading: boolean;
  onChange: (contractId: string) => void;
  onFiltersChange: (filters: AuditFilters) => void;
  onReset: () => void;
  onSubmit: () => void;
}

export function ContractSearch({
  contractId,
  filters,
  isLoading,
  onChange,
  onFiltersChange,
  onReset,
  onSubmit
}: ContractSearchProps) {
  const updateFilter = <TKey extends keyof AuditFilters>(key: TKey, value: AuditFilters[TKey]) => {
    onFiltersChange({
      ...filters,
      [key]: value
    });
  };

  return (
    <form
      className="search-bar"
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit();
      }}
    >
      <label className="contract-search-label">
        Numer / ID umowy
        <input
          className="contract-search-input"
          value={contractId}
          onChange={(event) => onChange(event.target.value)}
          placeholder="np. 123 albo UM-2026-001"
          autoComplete="off"
        />
      </label>
      <fieldset className="contract-filter-panel">
        <legend>Filtry</legend>
        <label className="contract-search-label">
          Data od
          <input
            className="contract-search-input"
            type="date"
            value={filters.from}
            onChange={(event) => updateFilter("from", event.target.value)}
          />
        </label>
        <label className="contract-search-label">
          Data do
          <input
            className="contract-search-input"
            type="date"
            value={filters.to}
            onChange={(event) => updateFilter("to", event.target.value)}
          />
        </label>
        <label className="contract-search-label">
          Typ zmiany
          <select
            className="contract-search-input"
            value={filters.changeType}
            onChange={(event) =>
              updateFilter("changeType", event.target.value as AuditFilters["changeType"])
            }
          >
            <option value="">Wszystkie</option>
            <option value="Added">Dodano</option>
            <option value="Modified">Zmieniono</option>
            <option value="Deleted">Usunięto</option>
          </select>
        </label>
        <label className="contract-search-label">
          Obiekt
          <select
            className="contract-search-input"
            value={filters.entityType}
            onChange={(event) =>
              updateFilter("entityType", event.target.value as AuditFilters["entityType"])
            }
          >
            <option value="">Wszystkie</option>
            <option value="ContractHeaderEntity">Umowa</option>
            <option value="AnnexHeaderEntity">Aneks</option>
            <option value="AnnexChangeEntity">Zmiana aneksu</option>
            <option value="FileEntity">Plik</option>
            <option value="InvoiceEntity">Faktura</option>
            <option value="PaymentScheduleEntity">Harmonogram płatności</option>
            <option value="ContractFundingEntity">Finansowanie</option>
            <option value="Unknown">Nieznany obiekt</option>
          </select>
        </label>
        <label className="contract-search-label">
          Użytkownik
          <input
            className="contract-search-input"
            value={filters.user}
            onChange={(event) => updateFilter("user", event.target.value)}
            placeholder="np. anna.nowak"
            autoComplete="off"
          />
        </label>
      </fieldset>
      <div className="contract-search-actions">
        <button type="submit" className="contract-search-button" disabled={isLoading}>
          <Search size={18} aria-hidden="true" />
          Szukaj
        </button>
        <button
          type="button"
          className="contract-search-button secondary"
          onClick={onReset}
          disabled={isLoading}
        >
          Wyczyść
        </button>
      </div>
    </form>
  );
}
