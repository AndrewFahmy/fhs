import { useState } from "react";
import { endpoints, settings } from "@/api/api-constants";
import { usePost } from "@/api/api-hooks";
import { fieldErrorsOf, lookupErrorFields } from "@/api/api-helpers";
import type {
    CreateCustomerRequest,
    CreateCustomerResponse,
    CreateStationRequest,
    CreateStationResponse,
} from "@/api/types";
import Banner from "@/components/common/banner";
import { Button } from "@/components/controls/button";
import Dialog from "@/components/controls/dialog";
import TextField from "@/components/controls/text-field";

const kinds = {
    station: {
        title: "New station",
        endpoint: endpoints.createStation,
        action: "Create station",
        codePlaceholder: "e.g. ST-04",
        namePlaceholder: "e.g. Weld cell 4",
    },
    customer: {
        title: "New customer",
        endpoint: endpoints.createCustomer,
        action: "Create customer",
        codePlaceholder: "e.g. C-014",
        namePlaceholder: "e.g. Alexandria Industrial Supply",
    },
};

export interface CreateLookupDialogProps {
    kind: keyof typeof kinds;
    onClose: () => void;
    onCreated: () => void;
}

/** Create form for the code-and-name lookup data: stations and customers. */
function CreateLookupDialog({
    kind,
    onClose,
    onCreated,
}: CreateLookupDialogProps) {
    const [code, setCode] = useState("");
    const [name, setName] = useState("");
    const { title, endpoint, action, codePlaceholder, namePlaceholder } =
        kinds[kind];

    const { post, loading, error } = usePost<
        CreateStationResponse | CreateCustomerResponse,
        CreateStationRequest | CreateCustomerRequest
    >(endpoint);

    const messages = fieldErrorsOf(error, lookupErrorFields);

    const otherError =
        error !== null && Object.keys(messages).length === 0
            ? (error.problem?.detail ?? error.message)
            : undefined;

    const complete = code.trim() !== "" && name.trim() !== "";

    async function submit() {
        const response = await post({ code, name });

        if (response) {
            onCreated();
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
                        disabled={loading || !complete}
                        onClick={() => void submit()}
                    >
                        {loading ? "Creating…" : action}
                    </Button>
                </>
            }
        >
            <div className="flex flex-col gap-5">
                <TextField
                    label="Code"
                    required
                    maxLength={settings.codeMaxLength}
                    placeholder={codePlaceholder}
                    disabled={loading}
                    error={messages.code}
                    value={code}
                    onChange={setCode}
                />
                <TextField
                    label="Name"
                    required
                    maxLength={settings.nameMaxLength}
                    placeholder={namePlaceholder}
                    disabled={loading}
                    error={messages.name}
                    value={name}
                    onChange={setName}
                />
            </div>

            {otherError ? (
                <div className="mt-5">
                    <Banner message={otherError} />
                </div>
            ) : null}
        </Dialog>
    );
}

export default CreateLookupDialog;
