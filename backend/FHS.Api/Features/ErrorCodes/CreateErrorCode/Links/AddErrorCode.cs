using FHS.Api.Data;
using FHS.Api.Data.Entities;
using FHS.Chain.Contracts;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.ErrorCodes.Links;

public sealed class AddErrorCode(FhsCommandDbContext db) : ILink<CreateErrorCodeState>
{
    public ValueTask<LinkResult> RunAsync(CreateErrorCodeState state, CancellationToken ct)
    {
        ErrorCode errorCode =
            new()
            {
                Code = state.Request.Code,
                Description = state.Request.Description,
                Severity = state.Request.Severity
            };

        db.Set<ErrorCode>().Add(errorCode);
        state.Produce(new CreateErrorCodeResponse(errorCode.Id));

        return ValueTask.FromResult(LinkResult.Continue);
    }
}
