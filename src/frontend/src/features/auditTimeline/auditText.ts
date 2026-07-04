import type { AuditTimelineItem } from "./types";

export function getAuditActionText(item: AuditTimelineItem): string {
  return `${item.changeTypeLabel}: ${item.entityLabel} - ${item.fieldLabel}`;
}

export function getTimelineTooltipText(item: AuditTimelineItem): string {
  return `Otwiera szczegóły zdarzenia. ${getAuditActionText(item)}. ${item.description}`;
}
