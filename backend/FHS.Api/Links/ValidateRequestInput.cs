using FHS.Api.Interfaces;
using FHS.Chain.Contracts;
using FHS.Chain.Enums;
using FHS.Chain.Primitives;
using FluentValidation;

namespace FHS.Api.Links;

public sealed class ValidateRequestInput<TRequest>(IValidator<TRequest> validator)
    : ILink<IHasRequest<TRequest>>
{
    public async ValueTask<LinkResult> RunAsync(IHasRequest<TRequest> state, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(state.Request, ct);

        if (result.IsValid)
        {
            return LinkResult.Continue;
        }

        var fields = result
            .Errors.Select(f => new FieldError(f.PropertyName, f.ErrorCode, f.ErrorMessage))
            .ToList();

        return LinkResult.Fail(
            new Error("Validation.Failed", "The Request is invalid.", ErrorKind.Validation, fields)
        );
    }
}
