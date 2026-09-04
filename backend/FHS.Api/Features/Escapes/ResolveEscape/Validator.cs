using FluentValidation;

namespace FHS.Api.Features.Escapes;

public sealed class ResolveEscapeValidator : AbstractValidator<ResolveEscapeRequest>
{
    public ResolveEscapeValidator()
    {
        RuleFor(r => r.Resolution).NotEmpty().MaximumLength(AppConstants.Data.EscapeResolutionMaxLength);
    }
}
