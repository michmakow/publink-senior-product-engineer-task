export type AuditChangeType = "" | "Added" | "Modified" | "Deleted";

export type AuditEntityType =
  | ""
  | "Unknown"
  | "ContractHeaderEntity"
  | "AnnexHeaderEntity"
  | "AnnexChangeEntity"
  | "FileEntity"
  | "InvoiceEntity"
  | "PaymentScheduleEntity"
  | "ContractFundingEntity";

export interface AuditFilters {
  from: string;
  to: string;
  changeType: AuditChangeType;
  entityType: AuditEntityType;
  user: string;
}

export interface AuditTimelineResponse {
  contractId: string;
  summary: AuditSummary;
  returnedItems: number;
  hasMoreItems: boolean;
  items: AuditTimelineItem[];
}

export interface ContractAuditSearchResponse {
  totalContracts: number;
  returnedContracts: number;
  hasMoreContracts: boolean;
  contracts: ContractAuditSearchItem[];
}

export interface ContractAuditSearchItem {
  contractId: string;
  contractNumber: string;
  summary: AuditSummary;
}

export interface AuditSummary {
  totalChanges: number;
  addedCount: number;
  modifiedCount: number;
  deletedCount: number;
  usersInvolved: number;
  firstChangeAt: string | null;
  lastChangeAt: string | null;
}

export interface AuditTimelineItem {
  id: string;
  changedAt: string;
  changedBy: string;
  entityType: string;
  entityLabel: string;
  changeType: string;
  changeTypeLabel: string;
  fieldName: string;
  fieldLabel: string;
  oldValue: string | null;
  newValue: string | null;
  description: string;
  isInitialState: boolean;
}

export const emptyAuditFilters: AuditFilters = {
  from: "",
  to: "",
  changeType: "",
  entityType: "",
  user: ""
};
