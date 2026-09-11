import { endpoints } from "@/api/api-constants";
import { useGet } from "@/api/api-hooks";
import type { StationListItem } from "@/api/types";
import Combobox, { type ComboboxProps } from "@/components/controls/combobox";

export type StationComboboxProps = Omit<
    ComboboxProps,
    "options" | "label" | "onChange"
> & {
    label?: string;
    includeInactive?: boolean;
    onChange: (code: string, station?: StationListItem) => void;
};

function StationCombobox({
    label = "Station",
    includeInactive = false,
    disabled = false,
    error,
    onChange,
    ...rest
}: StationComboboxProps) {
    const {
        data,
        loading,
        error: loadError,
    } = useGet<StationListItem[]>(
        includeInactive
            ? `${endpoints.getStations}?includeInactive=true`
            : endpoints.getStations,
    );

    const stations = data ?? [];

    return (
        <Combobox
            {...rest}
            label={label}
            disabled={disabled || loading}
            error={
                error ??
                (loadError ? "Stations could not be loaded." : undefined)
            }
            options={stations.map((station) => ({
                value: station.code,
                label: station.code,
                hint: station.name,
            }))}
            onChange={(code) =>
                onChange(
                    code,
                    stations.find((station) => station.code === code),
                )
            }
        />
    );
}

export default StationCombobox;
