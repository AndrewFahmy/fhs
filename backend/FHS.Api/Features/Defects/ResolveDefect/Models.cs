using FHS.Api.Interfaces;

namespace FHS.Api.Features.Defects;

public sealed record ResolveDefectRequest(string Resolution);

public sealed record DefectResolved(Guid DefectId, Guid ResolvedBy, DateTimeOffset OccurredAt) : IDomainEvent;
