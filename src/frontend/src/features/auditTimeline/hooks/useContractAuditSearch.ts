import { useCallback, useRef, useState } from "react";
import { ApiError, searchContractAudits } from "../../../api/auditTimelineApi";
import type { AuditFilters, ContractAuditSearchResponse } from "../types";

type SearchStatus = "idle" | "loading" | "success" | "error";

interface ContractAuditSearchState {
  data: ContractAuditSearchResponse | null;
  error: string | null;
  status: SearchStatus;
}

export function useContractAuditSearch() {
  const requestIdRef = useRef(0);
  const [state, setState] = useState<ContractAuditSearchState>({
    data: null,
    error: null,
    status: "idle"
  });

  const search = useCallback(async (contractId: string, filters: AuditFilters) => {
    const requestId = requestIdRef.current + 1;
    requestIdRef.current = requestId;

    setState((current) => ({
      ...current,
      data: null,
      error: null,
      status: "loading"
    }));

    try {
      const data = await searchContractAudits(contractId, filters);
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

      setState({
        data: null,
        error: error instanceof ApiError ? error.message : "Nie udało się wyszukać umów.",
        status: "error"
      });
    }
  }, []);

  const clear = useCallback(() => {
    requestIdRef.current += 1;
    setState({
      data: null,
      error: null,
      status: "idle"
    });
  }, []);

  return {
    ...state,
    clear,
    search
  };
}
