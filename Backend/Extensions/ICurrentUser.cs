using System.Security.Claims;
using Backend.Exceptions;

namespace Backend.Extensions;

public interface ICurrentUser {
    uint Id { get; }
}

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser {
    private bool _resolved;

    public uint Id {
        get {

            if (_resolved)
                return field;

            string? rawId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            field = uint.TryParse(rawId, out uint id) ? id : throw ServiceException.Unauthorized(Resources.ErrorAuthenticatedUserIdentityInvalid);
            _resolved = true;
            return field;
        }
    }
}