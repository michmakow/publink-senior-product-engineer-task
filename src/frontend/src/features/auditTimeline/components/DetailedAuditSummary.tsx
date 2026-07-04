import type { AuditTimelineItem, AuditTimelineResponse } from "../types";
import { getAuditActionText } from "../auditText";
import { formatDateTime } from "../format";
import "./DetailedAuditSummary.css";

interface DetailedAuditSummaryProps {
  data: AuditTimelineResponse;
  items: AuditTimelineItem[];
  embedded?: boolean;
}

export function DetailedAuditSummary({ data, embedded = false, items }: DetailedAuditSummaryProps) {
  const userActions = getUserActions(items);
  const modified = items.filter((item) => item.changeType === "Modified");
  const added = items.filter((item) => item.changeType === "Added");
  const deleted = items.filter((item) => item.changeType === "Deleted");

  return (
    <section
      className={`detailed-summary ${embedded ? "is-embedded" : ""}`}
      aria-label="Szczegółowa sumaryzacja historii"
    >
      <header>
        <div>
          <span>Sumaryzacja</span>
          <h2>Pełny obraz historii umowy {data.contractId}</h2>
        </div>
        <p>
          Zakres: <strong>{formatDateTime(data.summary.firstChangeAt)}</strong> -{" "}
          <strong>{formatDateTime(data.summary.lastChangeAt)}</strong>
        </p>
      </header>

      <div className="summary-counters">
        <Metric label="Wszystkie zmiany" value={data.summary.totalChanges} />
        <Metric label="Modyfikacje" value={data.summary.modifiedCount} />
        <Metric label="Dodania" value={data.summary.addedCount} />
        <Metric label="Usunięcia" value={data.summary.deletedCount} />
        <Metric label="Użytkownicy" value={data.summary.usersInvolved} />
      </div>

      <section className="summary-section">
        <h3>Użytkownicy</h3>
        {userActions.length === 0 ? (
          <p>Brak użytkowników w historii.</p>
        ) : (
          <dl className="user-action-list">
            {userActions.map(({ changedBy, actions }) => (
              <div className="user-action-card" key={changedBy}>
                <dt>{changedBy}</dt>
                <dd>
                  {actions.map((item) => (
                    <span key={item.id}>
                      {getAuditActionText(item)} ({formatDateTime(item.changedAt)})
                    </span>
                  ))}
                </dd>
              </div>
            ))}
          </dl>
        )}
      </section>

      <SummaryList
        title={`Modyfikacje: ${modified.length}`}
        emptyText="Brak modyfikacji."
        items={modified}
      />
      <SummaryList title={`Dodano: ${added.length}`} emptyText="Brak dodań." items={added} />
      <SummaryList
        title={`Usunięto: ${deleted.length}`}
        emptyText="Brak usunięć."
        items={deleted}
      />
    </section>
  );
}

function getUserActions(items: AuditTimelineItem[]) {
  const users = [...new Set(items.map((item) => item.changedBy))].sort((a, b) =>
    a.localeCompare(b, "pl")
  );

  return users.map((changedBy) => ({
    changedBy,
    actions: items.filter((item) => item.changedBy === changedBy)
  }));
}

function Metric({ label, value }: { label: string; value: number }) {
  return (
    <div className="summary-metric">
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function SummaryList({
  title,
  emptyText,
  items
}: {
  title: string;
  emptyText: string;
  items: AuditTimelineItem[];
}) {
  return (
    <section className="summary-section">
      <h3>{title}</h3>
      {items.length === 0 ? (
        <p>{emptyText}</p>
      ) : (
        <ul>
          {items.map((item) => (
            <li key={item.id}>
              <strong>{item.entityLabel}</strong>, {item.fieldLabel}:{" "}
              <span>{item.oldValue ?? "brak wartości"}</span> {"->"}{" "}
              <span>{item.newValue ?? "brak wartości"}</span> ({item.changedBy},{" "}
              {formatDateTime(item.changedAt)})
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
