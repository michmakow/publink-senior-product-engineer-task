import { useEffect, useMemo, useState } from "react";
import { AlertTriangle } from "lucide-react";
import { ContractSearch } from "./features/auditTimeline/components/ContractSearch";
import { ContractSearchResults } from "./features/auditTimeline/components/ContractSearchResults";
import { DetailedAuditSummary } from "./features/auditTimeline/components/DetailedAuditSummary";
import { EventCarousel } from "./features/auditTimeline/components/EventCarousel";
import { useContractAuditSearch } from "./features/auditTimeline/hooks/useContractAuditSearch";
import { useAuditTimeline } from "./features/auditTimeline/hooks/useAuditTimeline";
import {
  emptyAuditFilters,
  type AuditFilters,
  type AuditTimelineItem
} from "./features/auditTimeline/types";
import "./App.css";

type ViewMode = "detail" | "results";

export default function App() {
  const [contractId, setContractId] = useState("");
  const [filters, setFilters] = useState<AuditFilters>(emptyAuditFilters);
  const [appliedFilters, setAppliedFilters] = useState<AuditFilters>(emptyAuditFilters);
  const [expandedContractId, setExpandedContractId] = useState<string | null>(null);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [viewMode, setViewMode] = useState<ViewMode>("detail");
  const auditTimeline = useAuditTimeline();
  const contractSearch = useContractAuditSearch();

  const isDetailLoading = auditTimeline.status === "loading";
  const isSearchLoading = contractSearch.status === "loading";
  const isLoading = isDetailLoading || isSearchLoading;
  const appliedFiltersActive = hasAppliedFilters(appliedFilters);

  useEffect(() => {
    void auditTimeline.load("123", emptyAuditFilters);
  }, [auditTimeline.load]);

  useEffect(() => {
    setSelectedIndex(0);
  }, [auditTimeline.data?.contractId, auditTimeline.data?.items]);

  const timelineItems = useMemo(
    () => sortTimelineItems(auditTimeline.data?.items ?? []),
    [auditTimeline.data?.items]
  );

  const applySearch = () => {
    const filtersActive = hasAppliedFilters(filters);
    setSelectedIndex(0);
    setAppliedFilters(filtersActive ? filters : emptyAuditFilters);

    if (filtersActive) {
      setViewMode("results");
      setExpandedContractId(null);
      void contractSearch.search(contractId, filters);
      return;
    }

    contractSearch.clear();
    setViewMode("detail");
    setExpandedContractId(null);
    void auditTimeline.load(contractId.trim() || "123", emptyAuditFilters);
  };

  const resetSearch = () => {
    setContractId("");
    setFilters(emptyAuditFilters);
    setAppliedFilters(emptyAuditFilters);
    setExpandedContractId(null);
    setSelectedIndex(0);
    setViewMode("detail");
    contractSearch.clear();
    void auditTimeline.load("123", emptyAuditFilters);
  };

  const selectContract = (selectedContractId: string) => {
    if (expandedContractId === selectedContractId) {
      setExpandedContractId(null);
      return;
    }

    setExpandedContractId(selectedContractId);
    setSelectedIndex(0);
    void auditTimeline.load(selectedContractId, appliedFilters);
  };

  const directError = viewMode === "detail" ? auditTimeline.error : null;
  const searchError = viewMode === "results" ? contractSearch.error : null;

  return (
    <main className="app-shell">
      <section className="top-band">
        <div>
          <p className="eyebrow">Audit Timeline MVP</p>
          <h1>Historia zmian na umowie</h1>
        </div>
      </section>

      <section className="panel controls-panel">
        <ContractSearch
          contractId={contractId}
          filters={filters}
          isLoading={isLoading}
          onChange={setContractId}
          onFiltersChange={setFilters}
          onReset={resetSearch}
          onSubmit={applySearch}
        />
      </section>

      {(directError || searchError) && (
        <section className="status-banner" role="alert">
          <AlertTriangle size={20} aria-hidden="true" />
          <span>{directError ?? searchError}</span>
        </section>
      )}

      {viewMode === "detail" && isDetailLoading && (
        <section className="panel loading-state">Pobieranie historii zmian...</section>
      )}

      {viewMode === "results" && isSearchLoading && (
        <section className="panel loading-state">Wyszukiwanie umów...</section>
      )}

      {viewMode === "results" && contractSearch.data && !isSearchLoading && (
        <ContractSearchResults
          data={contractSearch.data}
          detailData={auditTimeline.data}
          detailError={auditTimeline.error}
          expandedContractId={expandedContractId}
          filtersApplied={appliedFiltersActive}
          isDetailLoading={isDetailLoading}
          selectedTimelineIndex={selectedIndex}
          onSelectContract={selectContract}
          onSelectTimelineItem={setSelectedIndex}
        />
      )}

      {viewMode === "detail" && auditTimeline.data && !isDetailLoading && (
        <>
          {auditTimeline.data.hasMoreItems && (
            <section className="status-banner" role="status">
              <AlertTriangle size={20} aria-hidden="true" />
              <span>
                Pokazano najnowsze {auditTimeline.data.returnedItems} wpisów. Starsze wpisy są
                poza zakresem widoku MVP.
              </span>
            </section>
          )}
          <EventCarousel
            items={timelineItems}
            selectedIndex={selectedIndex}
            onSelect={setSelectedIndex}
          />
          <DetailedAuditSummary data={auditTimeline.data} items={timelineItems} />
        </>
      )}
    </main>
  );
}

function hasAppliedFilters(filters: AuditFilters): boolean {
  return Boolean(
    filters.from.trim()
      || filters.to.trim()
      || filters.changeType
      || filters.entityType
      || filters.user.trim()
  );
}

function sortTimelineItems(items: readonly AuditTimelineItem[]) {
  return [...items].sort(
    (left, right) => new Date(left.changedAt).getTime() - new Date(right.changedAt).getTime()
  );
}
