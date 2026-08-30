using FHS.Api.Interfaces;
using FHS.Chain.Contracts;

namespace FHS.Api.Features.ErrorCodes;

public sealed class CreateErrorCodeState(CreateErrorCodeRequest request)
    : ChainState<CreateErrorCodeResponse>,
        IHasRequest<CreateErrorCodeRequest>
{
    public CreateErrorCodeRequest Request { get; } = request;
}
