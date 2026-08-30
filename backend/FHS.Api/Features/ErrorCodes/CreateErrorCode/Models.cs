using FHS.Api.Enums;

namespace FHS.Api.Features.ErrorCodes;

public sealed record CreateErrorCodeRequest(string Code, string Description, Severity Severity);

public sealed record CreateErrorCodeResponse(Guid ErrorCodeId);
