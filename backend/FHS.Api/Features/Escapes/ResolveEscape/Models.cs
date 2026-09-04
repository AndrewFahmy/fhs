using FHS.Api.Interfaces;

namespace FHS.Api.Features.Escapes;

public sealed record ResolveEscapeRequest(string Resolution);

public sealed record EscapeResolved(Guid EscapeId, Guid ResolvedBy, DateTimeOffset OccurredAt) : IDomainEvent;
