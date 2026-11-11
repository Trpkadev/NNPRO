using Backend.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Backend.Services;

public class TokenBlacklistService(IDistributedCache cache) : ITokenBlacklistService {
    private const string KeyPrefix = "jwt:blacklist:";

    public Task BlacklistTokenAsync(string jwtId, TimeSpan remainingLifetime, CancellationToken ct = default) => remainingLifetime <= TimeSpan.Zero ? Task.CompletedTask : cache.SetStringAsync(KeyPrefix + jwtId, "1", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = remainingLifetime }, ct);

    public async Task<bool> IsBlacklistedAsync(string jwtId, CancellationToken ct = default) => await cache.GetStringAsync(KeyPrefix + jwtId, ct) is not null;
}