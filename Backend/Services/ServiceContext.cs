using System.Diagnostics.CodeAnalysis;
using Backend.Configuration;
using Backend.Data;
using Backend.Extensions;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Backend.Services;

[SuppressMessage("Major Code Smell", "S6672:Generic logger injection should match enclosing type", Justification = "ServiceContext forwards ILogger<T> typed to the consuming service.")]
public sealed record ServiceContext<T>(
    AppDbContext Db,
    ICurrentUser CurrentUser,
    IOptions<DatabaseLimitsOptions> DbLimits,
    IOptions<Config> Cfg,
    ILogger<T> Logger,
    ITokenBlacklistService TokenBlacklistService,
    IOptions<JwtOptions> JwtOptions,
    JwtTokenGenerator JwtTokenGenerator) {
    public int MaxUnpagedItems => DbLimits.Value.MaxItemsLoadedWithoutPagination;
    public uint AuthUserId => CurrentUser.Id;
}