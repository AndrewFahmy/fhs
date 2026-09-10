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
