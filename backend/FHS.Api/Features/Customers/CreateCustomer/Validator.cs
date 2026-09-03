using FluentValidation;

namespace FHS.Api.Features.Customers;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerValidator()
    {
        RuleFor(r => r.Code).NotEmpty().MaximumLength(AppConstants.Data.CustomerCodeMaxLength);
        RuleFor(r => r.Name).NotEmpty().MaximumLength(AppConstants.Data.CustomerNameMaxLength);
    }
}
