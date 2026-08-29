using FluentValidation;

namespace FHS.Api.Features.Defects;

public sealed class ResolveDefectValidator : AbstractValidator<ResolveDefectRequest>
{
    public ResolveDefectValidator()
    {
        RuleFor(r => r.Resolution).NotEmpty().MaximumLength(AppConstants.Data.DefectResolutionMaxLength);
    }
}
