namespace FHS.Api.Features.Stations;

public sealed record CreateStationRequest(string FacilityCode, string Code, string Name);

public sealed record CreateStationResponse(Guid StationId);
