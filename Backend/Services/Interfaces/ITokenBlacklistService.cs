namespace Backend.Services.Interfaces;

public interface ITokenBlacklistService {
    /// <summary>
    ///     Adds a token ID to the shared cache until the token's remaining lifetime expires.
    /// </summary>
    Task BlacklistTokenAsync(string jwtId, TimeSpan remainingLifetime, CancellationToken ct = default);

    /// <summary>
    ///     Checks whether a token ID is present in the blacklist.
    /// </summary>
    Task<bool> IsBlacklistedAsync(string jwtId, CancellationToken ct = default);
}