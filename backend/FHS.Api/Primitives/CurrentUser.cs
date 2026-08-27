using System.Security.Claims;
using FHS.Api.Interfaces;

namespace FHS.Api.Primitives;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string? SubjectId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
}
