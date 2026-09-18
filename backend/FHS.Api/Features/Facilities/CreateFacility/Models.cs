namespace FHS.Api.Features.Facilities;

public sealed record CreateFacilityRequest(string Code, string Name);

public sealed record CreateFacilityResponse(Guid FacilityId);
