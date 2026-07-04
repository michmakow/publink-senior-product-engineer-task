import {
  ArrowLeft,
  ArrowRight,
  CalendarRange,
  FileText,
  Landmark,
  Pencil,
  Plus,
  ReceiptText,
  ShieldCheck,
  Trash2
} from "lucide-react";
import type { AuditTimelineItem } from "../types";
import { getAuditActionText, getTimelineTooltipText } from "../auditText";
import { formatDateOnly, formatDateTime } from "../format";
import "./EventCarousel.css";

interface EventCarouselProps {
  items: AuditTimelineItem[];
  selectedIndex: number;
  embedded?: boolean;
  onSelect: (index: number) => void;
}

export function EventCarousel({ items, selectedIndex, embedded = false, onSelect }: EventCarouselProps) {
  if (items.length === 0) {
    return (
      <section className="empty-state">
        <h2>Nie znaleziono historii zmian</h2>
        <p>Sprawdź numer umowy albo spróbuj ponownie.</p>
      </section>
    );
  }

  const selectedItem = items[Math.min(selectedIndex, items.length - 1)];
  const hasMultipleItems = items.length > 1;

  return (
    <section
      className={`event-workspace ${embedded ? "is-embedded" : ""}`}
      aria-label={hasMultipleItems ? "Timeline zdarzeń umowy" : "Szczegóły zdarzenia umowy"}
    >
      {hasMultipleItems && (
        <div className="timeline-strip">
          <div className="timeline-line" aria-hidden="true" />
          {items.map((item, index) => {
            const Icon = getEventIcon(item);
            const position = (index / (items.length - 1)) * 100;
            const tooltipId = `timeline-tooltip-${item.id}`;
            const tooltipText = getTimelineTooltipText(item);

            return (
              <button
                type="button"
                className={`timeline-dot ${index === selectedIndex ? "is-active" : ""} ${item.changeType.toLowerCase()}`}
                style={{ left: `${position}%` }}
                onClick={() => onSelect(index)}
                aria-label={`Pokaż szczegóły zdarzenia: ${item.description}`}
                aria-describedby={tooltipId}
                key={item.id}
              >
                <Icon size={18} aria-hidden="true" />
                <span className="timeline-date">{formatDateOnly(new Date(item.changedAt).getTime())}</span>
                <span className="timeline-tooltip" id={tooltipId} role="tooltip">
                  {tooltipText}
                </span>
              </button>
            );
          })}
        </div>
      )}

      <div className={`carousel-row ${hasMultipleItems ? "" : "is-single-event"}`}>
        {hasMultipleItems && (
          <button
            type="button"
            className="carousel-arrow"
            onClick={() => onSelect(Math.max(0, selectedIndex - 1))}
            disabled={selectedIndex === 0}
            aria-label="Poprzednie zdarzenie"
          >
            <ArrowLeft size={22} aria-hidden="true" />
          </button>
        )}

        <article className="event-card-active">
          <header>
            <div>
              <time dateTime={selectedItem.changedAt}>{formatDateTime(selectedItem.changedAt)}</time>
              <h2>{selectedItem.description}</h2>
            </div>
            <span className="event-kind">{selectedItem.changeTypeLabel}</span>
          </header>

          <dl className="event-details">
            <div>
              <dt>Użytkownik</dt>
              <dd className="event-user">
                <span>{selectedItem.changedBy}</span>
                <small>{getAuditActionText(selectedItem)}</small>
              </dd>
            </div>
            <div>
              <dt>Obiekt</dt>
              <dd>{selectedItem.entityLabel}</dd>
            </div>
            <div>
              <dt>Pole</dt>
              <dd>{selectedItem.fieldLabel}</dd>
            </div>
          </dl>

          <div className="event-change">
            <span>{selectedItem.oldValue ?? "brak wartości"}</span>
            <ArrowRight size={18} aria-hidden="true" />
            <strong>{selectedItem.newValue ?? "brak wartości"}</strong>
          </div>
        </article>

        {hasMultipleItems && (
          <button
            type="button"
            className="carousel-arrow"
            onClick={() => onSelect(Math.min(items.length - 1, selectedIndex + 1))}
            disabled={selectedIndex === items.length - 1}
            aria-label="Następne zdarzenie"
          >
            <ArrowRight size={22} aria-hidden="true" />
          </button>
        )}
      </div>
    </section>
  );
}

function getEventIcon(item: AuditTimelineItem) {
  if (item.isInitialState) {
    return ShieldCheck;
  }

  if (item.changeType === "Added") {
    return Plus;
  }

  if (item.changeType === "Deleted") {
    return Trash2;
  }

  if (item.entityType === "InvoiceEntity") {
    return ReceiptText;
  }

  if (item.entityType === "ContractFundingEntity") {
    return Landmark;
  }

  if (item.entityType === "FileEntity") {
    return FileText;
  }

  if (item.entityType === "PaymentScheduleEntity") {
    return CalendarRange;
  }

  return Pencil;
}
