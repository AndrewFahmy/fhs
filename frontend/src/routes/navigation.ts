export interface NavItem {
    label: string;
    to: string;
}

export interface NavGroup {
    label: string;
    match: string;
    items: NavItem[];
}

export type NavEntry = NavItem | NavGroup;

export function isNavGroup(entry: NavEntry): entry is NavGroup {
    return "items" in entry;
}

export const paths = {
    dashboard: "/",
    defects: "/defects",
    escapes: "/escapes",
    stations: "/stations",
    errorCodes: "/error-codes",
    customers: "/customers",
    newDefect: "/defects/new",
} as const;

export const navigation: NavEntry[] = [
    { label: "Defects", to: paths.defects },
    { label: "Escapes", to: paths.escapes },
    {
        label: "Reference",
        match: "/reference",
        items: [
            { label: "Stations", to: paths.stations },
            { label: "Error Codes", to: paths.errorCodes },
            { label: "Customers", to: paths.customers },
        ],
    },
];

export const flatNavigation: NavItem[] = navigation.flatMap((entry) =>
    isNavGroup(entry) ? entry.items : [entry],
);

export function defectPath(defectId: string): string {
    return `/defects/${defectId}`;
}