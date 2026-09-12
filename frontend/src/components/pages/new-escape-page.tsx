import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { endpoints, settings } from "@/api/api-constants";
import { usePost } from "@/api/api-hooks";
import type { Severity } from "@/api/enums";
import { fieldErrorsOf, escapeErrorFields } from "@/api/api-helpers";
import type { CreateEscapeRequest, CreateEscapeResponse } from "@/api/types";
import ErrorState from "@/components/common/error-state";
import SeverityMark from "@/components/common/severity-mark";
import { Button } from "@/components/controls/button";
import Textarea from "@/components/controls/textarea";
import CustomerCombobox from "@/components/lookups/customer-combobox";
import ErrorCodeCombobox from "@/components/lookups/error-code-combobox";
import BackLink from "@/components/navigation/back-link";
import { escapePath, paths } from "@/routes/navigation";
import { buttonClasses } from "@/utils/button-helpers";
import FormActions from "@/components/controls/form-actions";

const recordNotes = [
    {
        title: "Customer context stays explicit",
        detail: "This record is about what reached the customer.",
    },
    {
        title: "No inferred production origin",
        detail: "It never claims a station or internal defect link.",
    },
];

function NewEscapePage() {
    const navigate = useNavigate();

    const [customerCode, setCustomerCode] = useState("");
    const [errorCode, setErrorCode] = useState("");
    const [description, setDescription] = useState("");
    const [severity, setSeverity] = useState<Severity>();

    const { post, loading, error } = usePost<
        CreateEscapeResponse,
        CreateEscapeRequest
    >(endpoints.createEscape);

    const messages = fieldErrorsOf(error, escapeErrorFields);

    const complete =
        customerCode !== "" && errorCode !== "" && description.trim() !== "";

    async function submit() {
        const response = await post({ customerCode, errorCode, description });

        if (response) {
            void navigate(escapePath(response.data.escapeId), {
                replace: true,
            });
        }
    }

    if (error?.status === 403) {
        return <ErrorState error={error} title="You cannot report escapes." />;
    }

    return (
        <>
            <BackLink to={paths.escapes}>Escapes</BackLink>

            <header className="mt-6">
                <h1 className="font-display text-[38px] leading-tight font-bold text-ink">
                    Report an escape
                </h1>
                <p className="mt-1 text-sm text-ink-muted">
                    Capture the customer-reported fault and its classification.
                </p>
            </header>

            <div className="mt-8 grid gap-6 lg:grid-cols-[2fr_1fr]">
                <div className="rounded-md border border-border-strong bg-surface px-8 py-7">
                    <CustomerCombobox
                        variant="field"
                        className="w-full"
                        required
                        placeholder="Search customers"
                        disabled={loading}
                        error={messages.customercode}
                        value={customerCode}
                        onChange={setCustomerCode}
                    />

                    <div className="mt-7">
                        <ErrorCodeCombobox
                            variant="field"
                            className="w-full"
                            required
                            placeholder="Search error codes"
                            disabled={loading}
                            error={messages.errorcode}
                            value={errorCode}
                            onChange={(code, item) => {
                                setErrorCode(code);
                                setSeverity(item?.severity);
                            }}
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
                            label="Customer report"
                            required
                            rows={5}
                            maxLength={settings.descriptionMaxLength}
                            disabled={loading}
                            error={messages.description}
                            placeholder="Describe what the customer found or reported."
                            value={description}
                            onChange={setDescription}
                        />
                    </div>
                </div>

                <aside className="h-fit rounded-md border border-border-subtle bg-surface-subtle px-7 py-6">
                    <p className="text-[11px] font-bold uppercase tracking-wide text-ink-muted">
                        Escape record
                    </p>
                    <div className="mt-4 space-y-6 border-t border-border-subtle pt-5">
                        {recordNotes.map((note) => (
                            <div key={note.title}>
                                <p className="text-sm font-bold text-ink">
                                    {note.title}
                                </p>
                                <p className="mt-1 text-[13px] text-ink-muted">
                                    {note.detail}
                                </p>
                            </div>
                        ))}
                    </div>
                </aside>
            </div>

            <FormActions>
                <Link
                    to={paths.escapes}
                    className={buttonClasses({ variant: "link" })}
                >
                    Cancel
                </Link>
                <Button
                    disabled={loading || !complete}
                    onClick={() => void submit()}
                >
                    {loading ? "Reporting…" : "Report escape"}
                </Button>
            </FormActions>
        </>
    );
}

export default NewEscapePage;
