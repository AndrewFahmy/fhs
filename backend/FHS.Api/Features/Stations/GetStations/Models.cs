namespace FHS.Api.Features.Stations;

public sealed record StationListItem(Guid StationId, string Code, string Name, bool IsActive);
