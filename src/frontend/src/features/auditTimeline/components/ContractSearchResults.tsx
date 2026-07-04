import { useEffect, useMemo, useState, type CSSProperties } from "react";
import { ChevronDown, ChevronUp } from "lucide-react";
import type { ContractAuditSearchItem, ContractAuditSearchResponse } from "../types";
import type { AuditTimelineDetailsByContractId } from "../hooks/useAuditTimelineDetails";
import { formatDateOnly } from "../format";
import { DetailedAuditSummary } from "./DetailedAuditSummary";
import { EventCarousel } from "./EventCarousel";
import "./ContractSearchResults.css";

const sliderMax = 1000;

interface ContractSearchResultsProps {
  data: ContractAuditSearchResponse;
  detailsByContractId: AuditTimelineDetailsByContractId;
  expandedContractIds: string[];
  filtersApplied: boolean;
  selectedTimelineIndexes: Record<string, number>;
  onSelectContract: (contractId: string) => void;
  onSelectTimelineItem: (contractId: string, index: number) => void;
}

export function ContractSearchResults({
  data,
  detailsByContractId,
  expandedContractIds,
  filtersApplied,
  selectedTimelineIndexes,
  onSelectContract,
  onSelectTimelineItem
}: ContractSearchResultsProps) {
  const bounds = useMemo(() => getDateBounds(data.contracts), [data.contracts]);
  const expandedContractIdSet = useMemo(
    () => new Set(expandedContractIds),
    [expandedContractIds]
  );
  const [range, setRange] = useState(bounds);

  useEffect(() => {
    setRange(bounds);
  }, [bounds]);

  const showRangeSlider = filtersApplied && data.contracts.length > 1 && bounds !== null;
  const activeRange = range ?? bounds;
  const rangeStartPercent = bounds && activeRange
    ? getRangePercentage(activeRange.from, bounds)
    : 0;
  const rangeEndPercent = bounds && activeRange
    ? getRangePercentage(activeRange.to, bounds)
    : 100;
  const rangeStartValue = bounds && activeRange
    ? getSliderValue(activeRange.from, bounds)
    : 0;
  const rangeEndValue = bounds && activeRange
    ? getSliderValue(activeRange.to, bounds)
    : sliderMax;

  const visibleContracts = showRangeSlider && activeRange
    ? data.contracts.filter((contract) => isContractInRange(contract, activeRange.from, activeRange.to))
    : data.contracts;

  if (data.contracts.length === 0) {
    return (
      <section className="contract-results empty-results">
        <h2>Nie znaleziono umów</h2>
        <p>Zmień kryteria wyszukiwania i spróbuj ponownie.</p>
      </section>
    );
  }

  return (
    <section className="contract-results" aria-label="Znalezione umowy">
      <header>
        <div>
          <span>Wyniki</span>
          <h2>Znalezione umowy: {data.totalContracts}</h2>
        </div>
        {data.hasMoreContracts && <p>Pokazano pierwsze {data.returnedContracts} wyników.</p>}
      </header>

      {showRangeSlider && bounds && (
        <div
          className="result-range-slider"
          style={{
            "--range-start": `${rangeStartPercent}%`,
            "--range-end": `${rangeEndPercent}%`
          } as CSSProperties}
        >
          <div className="range-summary">
            <span>Zakres aktywności</span>
            <strong>
              {formatDateOnly(activeRange?.from ?? bounds.from)} -{" "}
              {formatDateOnly(activeRange?.to ?? bounds.to)}
            </strong>
          </div>
          <div className="date-range-control">
            <div className="date-range-labels" aria-hidden="true">
              <span>Od {formatDateOnly(activeRange?.from ?? bounds.from)}</span>
              <span>Do {formatDateOnly(activeRange?.to ?? bounds.to)}</span>
            </div>
            <div className="date-range-track" aria-hidden="true">
              <span className="date-range-track-base" />
              <span className="date-range-track-active" />
            </div>
            <input
              aria-label="Data od"
              className="date-range-input date-range-input-from"
              type="range"
              min={0}
              max={sliderMax}
              step={1}
              value={rangeStartValue}
              onChange={(event) => {
                const nextFrom = getTimestampFromSliderValue(Number(event.target.value), bounds);
                setRange((current) => {
                  const currentRange = current ?? bounds;
                  return {
                    from: Math.min(nextFrom, currentRange.to),
                    to: currentRange.to
                  };
                });
              }}
              disabled={bounds.from === bounds.to}
            />
            <input
              aria-label="Data do"
              className="date-range-input date-range-input-to"
              type="range"
              min={0}
              max={sliderMax}
              step={1}
              value={rangeEndValue}
              onChange={(event) => {
                const nextTo = getTimestampFromSliderValue(Number(event.target.value), bounds);
                setRange((current) => {
                  const currentRange = current ?? bounds;
                  return {
                    from: currentRange.from,
                    to: Math.max(nextTo, currentRange.from)
                  };
                });
              }}
              disabled={bounds.from === bounds.to}
            />
          </div>
        </div>
      )}

      <div className="contract-result-list">
        {visibleContracts.map((contract) => {
          const detailState = detailsByContractId[contract.contractId];
          const detailData = detailState?.data ?? null;
          const detailError = detailState?.error ?? null;
          const isDetailLoading = detailState?.status === "loading";
          const isExpanded = expandedContractIdSet.has(contract.contractId);
          const selectedTimelineIndex = selectedTimelineIndexes[contract.contractId] ?? 0;
          const hasLoadedDetail = detailData?.contractId === contract.contractId;
          const detailItems = hasLoadedDetail
            ? [...detailData.items].sort(
                (left, right) =>
                  new Date(left.changedAt).getTime() - new Date(right.changedAt).getTime()
              )
            : [];

          return (
            <article
              className={`contract-result-card ${isExpanded ? "is-expanded" : ""}`}
              key={contract.contractId}
            >
              <button
                type="button"
                className="contract-result-trigger"
                onClick={() => onSelectContract(contract.contractId)}
                aria-expanded={isExpanded}
              >
                <div className="contract-result-title">
                  <span>{contract.contractNumber}</span>
                  <strong>{contract.contractId}</strong>
                </div>
                <div className="contract-result-metrics">
                  <Metric label="Zmiany" value={contract.summary.totalChanges} />
                  <Metric label="Użytkownicy" value={contract.summary.usersInvolved} />
                  <Metric label="Ostatnia zmiana" value={formatDateOnlyValue(contract.summary.lastChangeAt)} />
                </div>
                {isExpanded ? (
                  <ChevronUp size={22} aria-hidden="true" />
                ) : (
                  <ChevronDown size={22} aria-hidden="true" />
                )}
              </button>

              {isExpanded && (
                <div className="contract-result-expanded">
                  {isDetailLoading && <div className="inline-loading">Pobieranie timeline...</div>}
                  {detailError && <div className="inline-error">{detailError}</div>}
                  {hasLoadedDetail && (
                    <>
                      {detailData.hasMoreItems && (
                        <div className="inline-warning">
                          Pokazano najnowsze {detailData.returnedItems} wpisów.
                        </div>
                      )}
                      <EventCarousel
                        embedded
                        items={detailItems}
                        selectedIndex={selectedTimelineIndex}
                        onSelect={(index) => onSelectTimelineItem(contract.contractId, index)}
                      />
                      <DetailedAuditSummary embedded data={detailData} items={detailItems} />
                    </>
                  )}
                </div>
              )}
            </article>
          );
        })}
      </div>
    </section>
  );
}

function Metric({ label, value }: { label: string; value: number | string }) {
  return (
    <span>
      <small>{label}</small>
      <strong>{value}</strong>
    </span>
  );
}

function formatDateOnlyValue(value: string | null): string {
  return value ? formatDateOnly(new Date(value).getTime()) : "brak daty";
}

function getDateBounds(contracts: ContractAuditSearchItem[]) {
  const timestamps = contracts
    .flatMap((contract) => [contract.summary.firstChangeAt, contract.summary.lastChangeAt])
    .filter((value): value is string => Boolean(value))
    .map((value) => new Date(value).getTime());

  if (timestamps.length === 0) {
    return null;
  }

  return {
    from: Math.min(...timestamps),
    to: Math.max(...timestamps)
  };
}

function getRangePercentage(value: number, bounds: { from: number; to: number }): number {
  if (bounds.to === bounds.from) {
    return 0;
  }

  return ((value - bounds.from) / (bounds.to - bounds.from)) * 100;
}

function getSliderValue(value: number, bounds: { from: number; to: number }): number {
  if (bounds.to === bounds.from) {
    return 0;
  }

  return Math.round(((value - bounds.from) / (bounds.to - bounds.from)) * sliderMax);
}

function getTimestampFromSliderValue(value: number, bounds: { from: number; to: number }): number {
  if (value <= 0) {
    return bounds.from;
  }

  if (value >= sliderMax) {
    return bounds.to;
  }

  return bounds.from + ((bounds.to - bounds.from) * value) / sliderMax;
}

function isContractInRange(contract: ContractAuditSearchItem, from: number, to: number): boolean {
  const first = contract.summary.firstChangeAt
    ? new Date(contract.summary.firstChangeAt).getTime()
    : null;
  const last = contract.summary.lastChangeAt
    ? new Date(contract.summary.lastChangeAt).getTime()
    : null;

  if (first === null || last === null) {
    return false;
  }

  return first <= to && last >= from;
}
