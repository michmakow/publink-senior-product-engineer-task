import type {
  AuditFilters,
  AuditTimelineResponse,
  ContractAuditSearchResponse
} from "../features/auditTimeline/types";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5080";
const apiKey = import.meta.env.VITE_AUDIT_API_KEY ?? "local-dev-audit-key";
const defaultErrorMessage = "Nie udało się pobrać historii zmian.";
const networkErrorMessage =
  "Nie udało się połączyć z API. Sprawdź, czy backend jest uruchomiony.";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string
  ) {
    super(message);
  }
}

export async function fetchContractAudit(
  contractId: string,
  filters: AuditFilters
): Promise<AuditTimelineResponse> {
  const url = new URL(
    `/api/contracts/${encodeURIComponent(contractId.trim())}/audit`,
    apiBaseUrl
  );

  appendQuery(url, "from", filters.from);
  appendQuery(url, "to", filters.to);
  appendQuery(url, "changeType", filters.changeType);
  appendQuery(url, "entityType", filters.entityType);
  appendQuery(url, "user", filters.user);

  const response = await sendAuditRequest(url);

  return response.json() as Promise<AuditTimelineResponse>;
}

export async function searchContractAudits(
  contractId: string,
  filters: AuditFilters
): Promise<ContractAuditSearchResponse> {
  const url = new URL("/api/contracts/audit-search", apiBaseUrl);

  appendQuery(url, "contractId", contractId);
  appendQuery(url, "from", filters.from);
  appendQuery(url, "to", filters.to);
  appendQuery(url, "changeType", filters.changeType);
  appendQuery(url, "entityType", filters.entityType);
  appendQuery(url, "user", filters.user);

  const response = await sendAuditRequest(url);

  return response.json() as Promise<ContractAuditSearchResponse>;
}

async function sendAuditRequest(url: URL): Promise<Response> {
  let response: Response;

  try {
    response = await fetch(url, {
      headers: {
        "X-Audit-Api-Key": apiKey
      }
    });
  } catch {
    throw new ApiError(0, networkErrorMessage);
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response));
  }

  return response;
}

function appendQuery(url: URL, key: string, value?: string): void {
  if (value?.trim()) {
    url.searchParams.set(key, value.trim());
  }
}

async function readErrorMessage(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as {
      message?: string;
      title?: string;
      errors?: Record<string, string[]>;
    };
    const validationMessage = body.errors ? Object.values(body.errors).flat()[0] : null;

    return validationMessage ?? body.message ?? body.title ?? defaultErrorMessage;
  } catch {
    return defaultErrorMessage;
  }
}
