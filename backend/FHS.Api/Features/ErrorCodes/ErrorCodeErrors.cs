using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.ErrorCodes;

public static class ErrorCodeErrors
{
    public static Error CodeAlreadyExists(string code) =>
        new("ErrorCodes.CodeAlreadyExists", $"An error code '{code}' already exists.", ErrorKind.Conflict);

    public static Error NotFound(Guid errorCodeId) =>
        new("ErrorCodes.NotFound", $"No error Code exists with id '{errorCodeId}'.", ErrorKind.NotFound);

    public static Error AlreadyRetired(Guid errorCodeId) =>
        new(
            "ErrorCodes.AlreadyRetired",
            $"Error code '{errorCodeId}' is already retired.",
            ErrorKind.Conflict
        );
}
