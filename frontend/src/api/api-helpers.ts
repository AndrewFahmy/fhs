import axios from "axios";
import { ApiError, type ApiProblem } from "@/api/types";

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

export function pageOf(search: URLSearchParams): number {
    const page = Number(search.get("page"));

    return Number.isInteger(page) && page > 0 ? page : 1;
}

/**
 * Field messages keyed by lower-cased field name: the validation `errors[]`,
 * plus any error `code` the caller maps onto a field.
 */
export function fieldErrorsOf(
    error: ApiError | null,
    codeFields: Record<string, string> = {},
): Partial<Record<string, string>> {
    if (!error) {
        return {};
    }

    const messages: Partial<Record<string, string>> = {};

    for (const fieldError of error.fieldErrors) {
        messages[fieldError.field.toLowerCase()] = fieldError.message;
    }

    const field = error.code ? codeFields[error.code] : undefined;

    if (field) {
        messages[field] = error.problem?.detail ?? error.message;
    }

    return messages;
}

/** Create-defect error codes that belong under a form field. */
export const defectErrorFields: Record<string, string> = {
    "Defects.StationNotFound": "stationcode",
    "Defects.StationInactive": "stationcode",
    "Defects.ErrorCodeNotFound": "errorcode",
    "Defects.ErrorCodeInactive": "errorcode",
};

/** Create-escape error codes that belong under a form field. */
export const escapeErrorFields: Record<string, string> = {
    "Escapes.CustomerNotFound": "customercode",
    "Escapes.CustomerInactive": "customercode",
    "Escapes.ErrorCodeNotFound": "errorcode",
    "Escapes.ErrorCodeInactive": "errorcode",
};
