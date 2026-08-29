using FluentValidation;

namespace FHS.Api.Features.Defects;

public sealed class CreateDefectValidator : AbstractValidator<CreateDefectRequest>
{
    public CreateDefectValidator()
    {
        RuleFor(r => r.StationCode).NotEmpty().MaximumLength(AppConstants.Data.StationCodeMaxLength);
        RuleFor(r => r.ErrorCode).NotEmpty().MaximumLength(AppConstants.Data.ErrorCodeMaxLength);
        RuleFor(r => r.Description).NotEmpty().MaximumLength(AppConstants.Data.DefectDescriptionMaxLength);
    }
}
