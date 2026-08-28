using FHS.Api.Data.Configuration;
using FluentValidation;

namespace FHS.Api.Features.Defects;

public sealed class CreateDefectValidator : AbstractValidator<CreateDefectRequest>
{
    public CreateDefectValidator()
    {
        RuleFor(r => r.StationCode).NotEmpty().MaximumLength(StationConfiguration.CodeMaxLength);
        RuleFor(r => r.ErrorCode).NotEmpty().MaximumLength(ErrorCodeConfiguration.CodeMaxLength);
        RuleFor(r => r.Description).NotEmpty().MaximumLength(DefectConfiguration.DescriptionMaxLength);
    }
}
