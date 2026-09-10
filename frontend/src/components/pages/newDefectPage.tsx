import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { endpoints, settings } from "@/api/api-constants";
import { useGet, usePost } from "@/api/api-hooks";
import { defectFieldErrors } from "@/api/helpers/defects";
import type {
    CreateDefectRequest,
    CreateDefectResponse,
    ErrorCodeListItem,
    StationListItem,
} from "@/api/types";
import ErrorState from "@/components/common/error-state";
import Icon from "@/components/common/icon";
import { Button } from "@/components/controls/button";
import Select from "@/components/controls/select";
import Textarea from "@/components/controls/textarea";
import SeverityMark from "@/components/defects/severity-mark";
import { defectPath, paths } from "@/routes/navigation";

const nextSteps = [
    {
        title: "The fault is classified",
        detail: "Severity is set from the selected code.",
    },
    {
        title: "The open fault enters the queue",
        detail: "It is visible to the next responsible operator.",
    },
];

function NewDefectPage() {
    const navigate = useNavigate();

    const [stationCode, setStationCode] = useState("");
    const [errorCode, setErrorCode] = useState("");
    const [description, setDescription] = useState("");

    const stations = useGet<StationListItem[]>(endpoints.getStations);
    const errorCodes = useGet<ErrorCodeListItem[]>(endpoints.getErrorCodes);

    const { post, loading, error } = usePost<
        CreateDefectResponse,
        CreateDefectRequest
    >(endpoints.createDefect);

    const messages = defectFieldErrors(error);
    const severity = errorCodes.data?.find(
        (item) => item.code === errorCode,
    )?.severity;

    const complete =
        stationCode !== "" && errorCode !== "" && description.trim() !== "";

    async function submit() {
        const response = await post({ stationCode, errorCode, description });

        if (response) {
            void navigate(defectPath(response.data.defectId), {
                replace: true,
            });
        }
    }

    if (error?.status === 403) {
        return <ErrorState error={error} title="You cannot raise defects." />;
    }

    return (
        <>
            <Link
                to={paths.defects}
                className="inline-flex items-center gap-2 text-sm font-bold uppercase text-ink-head hover:text-ink"
            >
                <Icon name="chevron-left" className="size-3" />
                Defects
            </Link>

            <header className="mt-6">
                <h1 className="font-display text-[38px] leading-tight font-bold text-ink">
                    Raise a defect
                </h1>
                <p className="mt-1 text-sm text-ink-muted">
                    Record the fault at the point it was found.
                </p>
            </header>

            <div className="mt-8 grid gap-6 lg:grid-cols-[2fr_1fr]">
                <div className="rounded-md border border-border-strong bg-surface px-8 py-7">
                    <Select
                        variant="field"
                        className="w-full"
                        label="Station"
                        required
                        placeholder="Select a station"
                        disabled={stations.loading || loading}
                        error={messages.stationcode}
                        value={stationCode}
                        options={(stations.data ?? []).map((station) => ({
                            value: station.code,
                            label: `${station.code} — ${station.name}`,
                        }))}
                        onChange={setStationCode}
                    />

                    <div className="mt-7">
                        <Select
                            variant="field"
                            className="w-full"
                            label="Error code"
                            required
                            placeholder="Select an error code"
                            disabled={errorCodes.loading || loading}
                            error={messages.errorcode}
                            value={errorCode}
                            options={(errorCodes.data ?? []).map((item) => ({
                                value: item.code,
                                label: `${item.code} — ${item.description}`,
                            }))}
                            onChange={setErrorCode}
                        />
                    </div>

                    {severity ? (
                        <div className="mt-4 flex flex-wrap items-center gap-5">
                            <SeverityMark severity={severity} badge />
                            <span className="text-[13px] text-ink-muted">
                                Severity comes from the error code.
                            </span>
                        </div>
                    ) : null}

                    <div className="mt-7">
                        <Textarea
                            label="Description"
                            required
                            rows={5}
                            maxLength={settings.descriptionMaxLength}
                            disabled={loading}
                            error={messages.description}
                            placeholder="Describe what was found and where."
                            value={description}
                            onChange={setDescription}
                        />
                    </div>
                </div>

                <aside className="h-fit rounded-md border border-border-subtle bg-surface-subtle px-7 py-6">
                    <p className="text-[11px] font-bold uppercase tracking-wide text-ink-muted">
                        What happens next
                    </p>
                    <div className="mt-4 border-t border-border-subtle pt-5">
                        {nextSteps.map((step, index) => (
                            <div
                                key={step.title}
                                className={`flex gap-4 ${index > 0 ? "mt-6" : ""}`}
                            >
                                <span className="flex size-5 shrink-0 items-center justify-center rounded-full bg-action text-[10px] font-bold text-action-ink">
                                    {index + 1}
                                </span>
                                <div>
                                    <p className="text-sm font-bold text-ink">
                                        {step.title}
                                    </p>
                                    <p className="mt-1 text-[13px] text-ink-muted">
                                        {step.detail}
                                    </p>
                                </div>
                            </div>
                        ))}
                    </div>
                </aside>
            </div>

            <div className="mt-8 flex items-center justify-end gap-4 border-t border-border-strong pt-6">
                <Link
                    to={paths.defects}
                    className="text-sm font-bold text-ink hover:underline"
                >
                    Cancel
                </Link>
                <Button
                    disabled={loading || !complete}
                    onClick={() => void submit()}
                >
                    {loading ? "Raising…" : "Raise defect"}
                </Button>
            </div>
        </>
    );
}

export default NewDefectPage;
