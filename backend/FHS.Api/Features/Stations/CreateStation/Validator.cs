using FluentValidation;

namespace FHS.Api.Features.Stations;

public sealed class CreateStationValidator : AbstractValidator<CreateStationRequest>
{
    public CreateStationValidator()
    {
        RuleFor(r => r.Code).NotEmpty().MaximumLength(AppConstants.Data.StationCodeMaxLength);
        RuleFor(r => r.Name).NotEmpty().MaximumLength(AppConstants.Data.StationNameMaxLength);
    }
}
