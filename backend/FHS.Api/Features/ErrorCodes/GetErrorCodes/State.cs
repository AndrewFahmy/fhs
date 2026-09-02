using FHS.Chain.Contracts;

namespace FHS.Api.Features.ErrorCodes;

public sealed class GetErrorCodesState(bool includeInactive) : ChainState<IReadOnlyList<ErrorCodeListItem>>
{
    public bool IncludeInactive { get; } = includeInactive;
}
