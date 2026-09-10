import { useState } from "react";
import { endpoints } from "@/api/api-constants";
import { usePost } from "@/api/api-hooks";
import type { DefectDetailResponse } from "@/api/types";
import Banner from "@/components/common/banner";
import Dialog from "@/components/controls/dialog";
import { Button } from "@/components/controls/button";
import Textarea from "@/components/controls/textarea";
import SeverityMark from "@/components/defects/severity-mark";
import { formatTimestamp, stringTemplateFormat } from "@/utils/format";

const maxResolutionLength = 500;

export interface ResolveDialogProps {
    defect: DefectDetailResponse;
    onClose: () => void;
    onResolved: () => void;
}

function ResolveDialog({ defect, onClose, onResolved }: ResolveDialogProps) {
    const [resolution, setResolution] = useState("");

    const { post, loading, error } = usePost<void, { resolution: string }>(
        stringTemplateFormat(endpoints.resolveDefect, defect.defectId),
    );

    const fieldError = error?.fieldErrors.find(
        (item) => item.field.toLowerCase() === "resolution",
    )?.message;

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
            title="Resolve defect"
            onClose={onClose}
            footer={
                <>
                    <Button variant="ghost" onClick={onClose}>
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
                <SeverityMark severity={defect.severity} />
                <span className="font-mono font-bold text-ink">
                    {defect.stationCode}
                </span>
                <span className="text-ink-muted">/</span>
                <span className="font-mono font-bold text-ink">
                    {defect.errorCode}
                </span>
            </div>
            <p className="mt-3 text-[13px] text-ink-muted">
                Raised {formatTimestamp(defect.createdAt)} by {defect.raisedBy}
            </p>

            <div className="mt-6">
                <Textarea
                    label="Resolution"
                    value={resolution}
                    maxLength={maxResolutionLength}
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
                            "This defect changed while you were typing."
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
