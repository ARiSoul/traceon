using System.Security.Claims;
using Traceon.Application.Interfaces;

namespace Traceon.Api.Services;

internal sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string UserId =>
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
}
