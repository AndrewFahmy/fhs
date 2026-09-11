import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { ErrorCodeListItem } from "@/api/types";
import Combobox, { type ComboboxProps } from "@/components/controls/combobox";

export type ErrorCodeComboboxProps = Omit<
    ComboboxProps,
    "options" | "label" | "onChange"
> & {
    label?: string;
    includeInactive?: boolean;
    onChange: (code: string, errorCode?: ErrorCodeListItem) => void;
};

function ErrorCodeCombobox({
    label = "Error code",
    includeInactive = false,
    disabled = false,
    error,
    onChange,
    ...rest
}: ErrorCodeComboboxProps) {
    const {
        data,
        loading,
        error: loadError,
    } = useGet<ErrorCodeListItem[]>(
        includeInactive
            ? `${endpoints.getErrorCodes}?includeInactive=true`
            : endpoints.getErrorCodes,
    );

    const errorCodes = data ?? [];

    return (
        <Combobox
            {...rest}
            label={label}
            disabled={disabled || loading}
            error={
                error ??
                (loadError ? "Error codes could not be loaded." : undefined)
            }
            options={errorCodes.map((errorCode) => ({
                value: errorCode.code,
                label: errorCode.code,
                hint: errorCode.description,
            }))}
            onChange={(code) =>
                onChange(
                    code,
                    errorCodes.find((errorCode) => errorCode.code === code),
                )
            }
        />
    );
}

export default ErrorCodeCombobox;
