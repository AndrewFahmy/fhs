using FluentValidation;

namespace FHS.Api.Features.ErrorCodes;

public sealed class CreateErrorCodeValidator : AbstractValidator<CreateErrorCodeRequest>
{
    public CreateErrorCodeValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(AppConstants.Data.ErrorCodeMaxLength);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(AppConstants.Data.ErrorCodeDescriptionMaxLength);
        RuleFor(x => x.Severity).IsInEnum();
    }
}
