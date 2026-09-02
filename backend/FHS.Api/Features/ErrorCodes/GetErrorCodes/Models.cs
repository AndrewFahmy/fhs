using FHS.Api.Enums;

namespace FHS.Api.Features.ErrorCodes;

public sealed record ErrorCodeListItem(
    Guid ErrorCodeId,
    string Code,
    string Description,
    Severity Severity,
    bool IsActive
);
