import { useState } from "react";
import { endpoints, settings } from "@/api/api-constants";
import { usePost } from "@/api/api-hooks";
import { fieldErrorsOf, lookupErrorFields } from "@/api/api-helpers";
import { severityOptions, type Severity } from "@/api/enums";
import type {
    CreateErrorCodeRequest,
    CreateErrorCodeResponse,
} from "@/api/types";
import Banner from "@/components/common/banner";
import { Button } from "@/components/controls/button";
import Dialog from "@/components/controls/dialog";
import Select from "@/components/controls/select";
import TextField from "@/components/controls/text-field";

export interface CreateErrorCodeDialogProps {
    onClose: () => void;
    onCreated: () => void;
}

function CreateErrorCodeDialog({
    onClose,
    onCreated,
}: CreateErrorCodeDialogProps) {
    const [code, setCode] = useState("");
    const [description, setDescription] = useState("");
    const [severity, setSeverity] = useState<Severity | "">("");

    const { post, loading, error } = usePost<
        CreateErrorCodeResponse,
        CreateErrorCodeRequest
    >(endpoints.createErrorCode);

    const messages = fieldErrorsOf(error, lookupErrorFields);

    const otherError =
        error !== null && Object.keys(messages).length === 0
            ? (error.problem?.detail ?? error.message)
            : undefined;

    const complete =
        code.trim() !== "" && description.trim() !== "" && severity !== "";

    async function submit() {
        if (severity === "") {
            return;
        }

        const response = await post({ code, description, severity });

        if (response) {
            onCreated();
        }
    }

    return (
        <Dialog
            title="New error code"
            onClose={onClose}
            footer={
                <>
                    <Button variant="link" onClick={onClose}>
                        Cancel
                    </Button>
                    <Button
                        disabled={loading || !complete}
                        onClick={() => void submit()}
                    >
                        {loading ? "Creating…" : "Create error code"}
                    </Button>
                </>
            }
        >
            <div className="flex flex-col gap-5">
                <TextField
                    label="Code"
                    required
                    maxLength={settings.codeMaxLength}
                    placeholder="e.g. EC-1021"
                    disabled={loading}
                    error={messages.code}
                    value={code}
                    onChange={setCode}
                />
                <TextField
                    label="Description"
                    required
                    maxLength={settings.descriptionMaxLength}
                    placeholder="e.g. Weld porosity"
                    disabled={loading}
                    error={messages.description}
                    value={description}
                    onChange={setDescription}
                />
                <Select
                    label="Severity"
                    variant="field"
                    className="w-full"
                    required
                    placeholder="Choose a severity"
                    disabled={loading}
                    error={messages.severity}
                    value={severity}
                    options={severityOptions}
                    onChange={(value) => setSeverity(value as Severity | "")}
                />
            </div>

            <p className="mt-4 text-[13px] text-ink-muted">
                Defects and escapes classified with this code take its
                severity.
            </p>

            {otherError ? (
                <div className="mt-5">
                    <Banner message={otherError} />
                </div>
            ) : null}
        </Dialog>
    );
}

export default CreateErrorCodeDialog;
