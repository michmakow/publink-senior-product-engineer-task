import { useCallback, useRef, useState } from "react";
import { ApiError, fetchContractAudit } from "../../../api/auditTimelineApi";
import type { AuditFilters, AuditTimelineResponse } from "../types";

type LoadStatus = "idle" | "loading" | "success" | "notFound" | "error";

interface AuditTimelineState {
  data: AuditTimelineResponse | null;
  error: string | null;
  status: LoadStatus;
}

export function useAuditTimeline() {
  const requestIdRef = useRef(0);
  const [state, setState] = useState<AuditTimelineState>({
    data: null,
    error: null,
    status: "idle"
  });

  const load = useCallback(async (contractId: string, filters: AuditFilters) => {
    const requestId = requestIdRef.current + 1;
    requestIdRef.current = requestId;

    if (!contractId.trim()) {
      setState({
        data: null,
        error: "Podaj numer albo ID umowy.",
        status: "error"
      });
      return;
    }

    setState((current) => ({
      ...current,
      data: null,
      error: null,
      status: "loading"
    }));

    try {
      const data = await fetchContractAudit(contractId, filters);
      if (requestId !== requestIdRef.current) {
        return;
      }

      setState({
        data,
        error: null,
        status: "success"
      });
    } catch (error) {
      if (requestId !== requestIdRef.current) {
        return;
      }

      if (error instanceof ApiError && error.status === 404) {
        setState({
          data: null,
          error: error.message,
          status: "notFound"
        });
        return;
      }

      setState({
        data: null,
        error:
          error instanceof ApiError ? error.message : "Nie udało się pobrać historii zmian.",
        status: "error"
      });
    }
  }, []);

  return {
    ...state,
    load
  };
}
