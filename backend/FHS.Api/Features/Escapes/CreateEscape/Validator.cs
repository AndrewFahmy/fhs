using FluentValidation;

namespace FHS.Api.Features.Escapes;

public sealed class CreateEscapeValidator : AbstractValidator<CreateEscapeRequest>
{
    public CreateEscapeValidator()
    {
        RuleFor(r => r.CustomerCode).NotEmpty().MaximumLength(AppConstants.Data.CustomerCodeMaxLength);
        RuleFor(r => r.ErrorCode).NotEmpty().MaximumLength(AppConstants.Data.ErrorCodeMaxLength);
        RuleFor(r => r.Description).NotEmpty().MaximumLength(AppConstants.Data.EscapeDescriptionMaxLength);
    }
}
