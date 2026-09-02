using FHS.Chain.Contracts;

namespace FHS.Api.Features.Defects;

public sealed class GetDefectState(Guid defectId) : ChainState<DefectDetailResponse>
{
    public Guid DefectId { get; } = defectId;
}
