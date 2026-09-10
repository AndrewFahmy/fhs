import { settings } from "@/api/api-constants";
import type { DefectStatus } from "@/api/enums";
import type { ApiError } from "@/api/types";

export function parseDefectStatusFilter(search: URLSearchParams): DefectStatus {
    const status = search.get("status");

    return status === "resolved" || status === "all" ? status : "open";
}

export function defectsPageHasFilters(search: URLSearchParams): boolean {
    return (
        parseDefectStatusFilter(search) !== "open" ||
        ["stationCode", "errorCode", "severity"].some(
            (key) => search.get(key) !== null,
        )
    );
}

export function mapQuery(search: URLSearchParams): string {
    const params = new URLSearchParams();

    for (const key of ["stationCode", "errorCode", "severity", "page"]) {
        const value = search.get(key);

        if (value) params.set(key, value);
    }

    params.set("pageSize", String(settings.defaultPageSize));

    const status = parseDefectStatusFilter(search);

    if (status !== "all") {
        params.set("isResolved", String(status === "resolved"));
    }

    return params.toString();
}

export function defectFieldErrors(
    error: ApiError | null,
): Record<string, string> {
    if (!error) {
        return {};
    }

    const messages: Record<string, string> = {};

    for (const fieldError of error.fieldErrors) {
        messages[fieldError.field.toLowerCase()] = fieldError.message;
    }

    const detail = error.problem?.detail ?? error.message;

    if (
        error.code === "Defects.StationNotFound" ||
        error.code === "Defects.StationInactive"
    ) {
        messages.stationcode = detail;
    }

    if (
        error.code === "Defects.ErrorCodeNotFound" ||
        error.code === "Defects.ErrorCodeInactive"
    ) {
        messages.errorcode = detail;
    }

    return messages;
}
