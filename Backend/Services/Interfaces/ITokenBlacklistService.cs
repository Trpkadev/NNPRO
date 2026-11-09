namespace Backend.Services.Interfaces;

public interface ITokenBlacklistService {
    /// <summary>
    ///     Zaznamená JTI zablokovaného Access Tokenu do sdílené cache s expirací nastavenou na zbývající čas tokenu.
    /// </summary>
    Task BlacklistTokenAsync(string jwtId, TimeSpan remainingLifetime, CancellationToken ct = default);

    /// <summary>
    ///     Ověří, zda je daný JTI přítomen na blacklistu (voláno v JWT Bearer middleware).
    /// </summary>
    Task<bool> IsBlacklistedAsync(string jwtId, CancellationToken ct = default);
}