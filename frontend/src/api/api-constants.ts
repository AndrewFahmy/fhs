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
    createStation: "/stations",
    decommissionStation: "/stations/{0}/decommission",
    getErrorCodes: "/error-codes",
    createErrorCode: "/error-codes",
    retireErrorCode: "/error-codes/{0}/retire",
    getCustomers: "/customers",
    createCustomer: "/customers",
    deactivateCustomer: "/customers/{0}/deactivate",
};

export const settings = {
    defaultPageSize: 20,
    descriptionMaxLength: 500,
    resolutionMaxLength: 500,
    codeMaxLength: 50,
    nameMaxLength: 200,
};
