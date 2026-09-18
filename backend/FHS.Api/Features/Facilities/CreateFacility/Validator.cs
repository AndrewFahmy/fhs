using FluentValidation;

namespace FHS.Api.Features.Facilities;

public sealed class CreateFacilityValidator : AbstractValidator<CreateFacilityRequest>
{
    public CreateFacilityValidator()
    {
        RuleFor(r => r.Code).NotEmpty().MaximumLength(AppConstants.Data.FacilityCodeMaxLength);
        RuleFor(r => r.Name).NotEmpty().MaximumLength(AppConstants.Data.FacilityNameMaxLength);
    }
}
