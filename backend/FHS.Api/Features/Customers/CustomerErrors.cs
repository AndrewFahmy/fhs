using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Customers;

public static class CustomerErrors
{
    public static Error CodeAlreadyExists(string code) =>
        new(
            "Customers.CodeAlreadyExists",
            $"A customer with code '{code}' already exists.",
            ErrorKind.Conflict
        );

    public static Error NotFound(Guid customerId) =>
        new("Customers.NotFound", $"No customer exists with id '{customerId}'.", ErrorKind.NotFound);

    public static Error AlreadyDeactivated(Guid customerId) =>
        new(
            "Customers.AlreadyDeactivated",
            $"Customer '{customerId}' is already deactivated.",
            ErrorKind.Conflict
        );
}
