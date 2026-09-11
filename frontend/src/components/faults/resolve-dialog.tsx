import { useState } from "react";
import { endpoints, settings } from "@/api/api-constants";
import { usePost } from "@/api/api-hooks";
import type { Severity } from "@/api/enums";
import Banner from "@/components/common/banner";
import SeverityMark from "@/components/common/severity-mark";
import { Button } from "@/components/controls/button";
import Dialog from "@/components/controls/dialog";
import Textarea from "@/components/controls/textarea";
import { formatTimestamp, stringTemplateFormat } from "@/utils/format";
import { fieldErrorsOf } from "@/api/api-helpers";

const kinds = {
    defect: {
        title: "Resolve defect",
        endpoint: endpoints.resolveDefect,
        opened: "Raised",
    },
    escape: {
        title: "Resolve escape",
        endpoint: endpoints.resolveEscape,
        opened: "Reported",
    },
};

export interface ResolveDialogProps {
    kind: keyof typeof kinds;
    id: string;
    severity: Severity;
    /** Station code for a defect, customer code for an escape. */
    sourceCode: string;
    errorCode: string;
    openedAt: string;
    openedBy: string;
    onClose: () => void;
    onResolved: () => void;
}

function ResolveDialog({
    kind,
    id,
    severity,
    sourceCode,
    errorCode,
    openedAt,
    openedBy,
    onClose,
    onResolved,
}: ResolveDialogProps) {
    const [resolution, setResolution] = useState("");
    const { title, endpoint, opened } = kinds[kind];

    const { post, loading, error } = usePost<void, { resolution: string }>(
        stringTemplateFormat(endpoint, id),
    );

    const fieldError = fieldErrorsOf(error).resolution;

    const stale =
        error !== null && (error.status === 409 || error.status === 404);

    const otherError =
        error !== null && !stale && fieldError === undefined
            ? error.message
            : undefined;

    async function submit() {
        const response = await post({ resolution });

        if (response) {
            onResolved();
        }
    }

    return (
        <Dialog
            title={title}
            onClose={onClose}
            footer={
                <>
                    <Button variant="link" onClick={onClose}>
                        Cancel
                    </Button>
                    <Button
                        disabled={loading || resolution.trim().length === 0}
                        onClick={() => void submit()}
                    >
                        {loading ? "Resolving…" : "Mark resolved"}
                    </Button>
                </>
            }
        >
            <div className="flex items-center gap-3 text-[13px]">
                <SeverityMark severity={severity} />
                <span className="font-mono font-bold text-ink">
                    {sourceCode}
                </span>
                <span className="text-ink-muted">/</span>
                <span className="font-mono font-bold text-ink">
                    {errorCode}
                </span>
            </div>
            <p className="mt-3 text-[13px] text-ink-muted">
                {opened} {formatTimestamp(openedAt)} by {openedBy}
            </p>

            <div className="mt-6">
                <Textarea
                    label="Resolution"
                    value={resolution}
                    maxLength={settings.resolutionMaxLength}
                    disabled={loading}
                    error={fieldError}
                    placeholder="Describe the corrective action taken"
                    onChange={setResolution}
                />
            </div>

            {stale ? (
                <div className="mt-5">
                    <Banner
                        message={
                            error.problem?.detail ??
                            `This ${kind} changed while you were typing.`
                        }
                        action={
                            <Button
                                variant="secondary"
                                size="sm"
                                onClick={onResolved}
                            >
                                Reload
                            </Button>
                        }
                    />
                </div>
            ) : null}

            {otherError ? (
                <div className="mt-5">
                    <Banner message={otherError} />
                </div>
            ) : null}
        </Dialog>
    );
}

export default ResolveDialog;
