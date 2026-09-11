export const endpoints = {
    getDefects: "/defects",
    createDefect: "/defects",
    getDefectDetails: "/defects/{0}",
    resolveDefect: "/defects/{0}/resolve",
    getEscapes: "/escapes",
    createEscape: "/escapes",
    getEscapeDetails: "/escapes/{0}",
    resolveEscape: "/escapes/{0}/resolve",
    getStations: "/stations",
    getErrorCodes: "/error-codes",
    getCustomers: "/customers",
};

export const settings = {
    defaultPageSize: 20,
    descriptionMaxLength: 500,
    resolutionMaxLength: 500,
};
