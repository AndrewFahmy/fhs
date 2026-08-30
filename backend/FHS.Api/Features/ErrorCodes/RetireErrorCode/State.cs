using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.ErrorCodes;

public sealed class RetireErrorCodeState(Guid errorCodeId) : ChainState
{
    public Guid ErrorCodeId { get; } = errorCodeId;

    public ErrorCode? ErrorCode { get; set; }
}
