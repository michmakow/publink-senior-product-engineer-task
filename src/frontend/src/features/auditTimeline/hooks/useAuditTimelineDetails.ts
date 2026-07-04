import { useCallback, useRef, useState } from "react";
import { ApiError, fetchContractAudit } from "../../../api/auditTimelineApi";
import type { AuditFilters, AuditTimelineResponse } from "../types";

type DetailStatus = "idle" | "loading" | "success" | "notFound" | "error";

export interface AuditTimelineDetailState {
  data: AuditTimelineResponse | null;
  error: string | null;
  status: DetailStatus;
}

export type AuditTimelineDetailsByContractId = Record<string, AuditTimelineDetailState>;

export function useAuditTimelineDetails() {
  const requestIdsRef = useRef<Record<string, number>>({});
  const requestCounterRef = useRef(0);
  const [detailsByContractId, setDetailsByContractId] =
    useState<AuditTimelineDetailsByContractId>({});

  const load = useCallback(async (contractId: string, filters: AuditFilters) => {
    const normalizedContractId = contractId.trim();
    if (!normalizedContractId) {
      return;
    }

    requestCounterRef.current += 1;
    const requestId = requestCounterRef.current;
    requestIdsRef.current[normalizedContractId] = requestId;

    setDetailsByContractId((current) => ({
      ...current,
      [normalizedContractId]: {
        data: null,
        error: null,
        status: "loading"
      }
    }));

    try {
      const data = await fetchContractAudit(normalizedContractId, filters);
      if (requestIdsRef.current[normalizedContractId] !== requestId) {
        return;
      }

      setDetailsByContractId((current) => ({
        ...current,
        [normalizedContractId]: {
          data,
          error: null,
          status: "success"
        }
      }));
    } catch (error) {
      if (requestIdsRef.current[normalizedContractId] !== requestId) {
        return;
      }

      setDetailsByContractId((current) => ({
        ...current,
        [normalizedContractId]: {
          data: null,
          error: error instanceof ApiError ? error.message : "Nie udało się pobrać historii zmian.",
          status: error instanceof ApiError && error.status === 404 ? "notFound" : "error"
        }
      }));
    }
  }, []);

  const clear = useCallback(() => {
    requestCounterRef.current += 1;
    requestIdsRef.current = {};
    setDetailsByContractId({});
  }, []);

  return {
    clear,
    detailsByContractId,
    load
  };
}
