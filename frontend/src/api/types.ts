import { type InternalAxiosRequestConfig } from "axios";
import type { Severity } from "@/api/enums";

export interface FieldError {
    field: string;
    code: string;
    message: string;
}

export interface ApiProblem {
    type?: string;
    title?: string;
    status?: number;
    detail?: string;
    instance?: string;
    code: string;
    errors?: FieldError[];
    path?: string;
}

export interface PagedResponse<TItem> {
    items: TItem[];
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}

export class ApiError extends Error {
    readonly status: number;
    readonly problem: ApiProblem | undefined;

    constructor(status: number, problem: ApiProblem | undefined) {
        super(problem?.title ?? `The request failed (${status}).`);
        this.name = "ApiError";
        this.status = status;
        this.problem = problem;
    }

    /** Branch on this, never on the status text. */
    get code(): string | undefined {
        return this.problem?.code;
    }

    get fieldErrors(): FieldError[] {
        return this.problem?.errors ?? [];
    }
}

export interface RetriedRequestConfig extends InternalAxiosRequestConfig {
    retried?: boolean;
}

export interface DefectListItem {
    defectId: string;
    stationCode: string;
    errorCode: string;
    severity: Severity;
    description: string;
    raisedBy: string;
    createdAt: string;
    resolvedAt: string | null;
}

export interface StationListItem {
    stationId: string;
    code: string;
    name: string;
    isActive: boolean;
}

export interface ErrorCodeListItem {
    errorCodeId: string;
    code: string;
    description: string;
    severity: Severity;
    isActive: boolean;
}

export interface DefectDetailResponse {
    defectId: string;
    stationId: string;
    stationCode: string;
    stationName: string;
    errorCodeId: string;
    errorCode: string;
    errorCodeDescription: string;
    severity: Severity;
    description: string;
    raisedBy: string;
    createdAt: string;
    resolution: string | null;
    resolvedBy: string | null;
    resolvedAt: string | null;
}

export interface CreateDefectRequest {
    stationCode: string;
    errorCode: string;
    description: string;
}

export interface CreateDefectResponse {
    defectId: string;
}
