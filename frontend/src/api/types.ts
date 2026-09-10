import axios, { type InternalAxiosRequestConfig } from "axios";
import type { Severity } from "./enums";

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

function isApiProblem(value: unknown): value is ApiProblem {
    return (
        typeof value === "object" &&
        value !== null &&
        "code" in value &&
        typeof value.code === "string"
    );
}

export function toApiError(cause: unknown): ApiError {
    if (cause instanceof ApiError) {
        return cause;
    }

    if (axios.isAxiosError(cause)) {
        const status = cause.response?.status ?? 0;
        const problem = isApiProblem(cause.response?.data)
            ? cause.response.data
            : undefined;

        if (problem) {
            return new ApiError(status, problem);
        }

        return new ApiError(status, {
            code: cause.response
                ? "Http.UnexpectedResponse"
                : "Network.Unreachable",
            title: cause.response
                ? `The API returned an unexpected response (${status}).`
                : cause.message || "The request could not be sent.",
        });
    }

    return new ApiError(0, {
        code: "Network.Unreachable",
        title:
            cause instanceof Error
                ? cause.message
                : "The request could not be sent.",
    });
}
