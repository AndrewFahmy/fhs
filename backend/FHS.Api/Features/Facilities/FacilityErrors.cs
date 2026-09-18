using FHS.Chain.Enums;
using FHS.Chain.Primitives;

namespace FHS.Api.Features.Facilities;

public static class FacilityErrors
{
    public static Error CodeAlreadyExists(string code) =>
        new(
            "Facilities.CodeAlreadyExists",
            $"A facility with code '{code}' already exists.",
            ErrorKind.Conflict
        );
}
