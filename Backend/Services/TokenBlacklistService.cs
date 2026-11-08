using Backend.Services.Interfaces;

namespace Backend.Services;

public class TokenBlacklistService : ITokenBlacklistService {
    public Task BlacklistTokenAsync(string jwtId, TimeSpan remainingLifetime, CancellationToken ct = default) => throw new NotImplementedException();

    public Task<bool> IsBlacklistedAsync(string jwtId, CancellationToken ct = default) => throw new NotImplementedException();
}