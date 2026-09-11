import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { CustomerListItem } from "@/api/types";
import Combobox, { type ComboboxProps } from "@/components/controls/combobox";

export type CustomerComboboxProps = Omit<
    ComboboxProps,
    "options" | "label" | "onChange"
> & {
    label?: string;
    includeInactive?: boolean;
    onChange: (code: string, customer?: CustomerListItem) => void;
};

function CustomerCombobox({
    label = "Customer",
    includeInactive = false,
    disabled = false,
    error,
    onChange,
    ...rest
}: CustomerComboboxProps) {
    const {
        data,
        loading,
        error: loadError,
    } = useGet<CustomerListItem[]>(
        includeInactive
            ? `${endpoints.getCustomers}?includeInactive=true`
            : endpoints.getCustomers,
    );

    const customers = data ?? [];

    return (
        <Combobox
            {...rest}
            label={label}
            disabled={disabled || loading}
            error={
                error ??
                (loadError ? "Customers could not be loaded." : undefined)
            }
            options={customers.map((customer) => ({
                value: customer.code,
                label: customer.code,
                hint: customer.name,
            }))}
            onChange={(code) =>
                onChange(
                    code,
                    customers.find((customer) => customer.code === code),
                )
            }
        />
    );
}

export default CustomerCombobox;
