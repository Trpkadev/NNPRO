using Backend.Configuration;
using Backend.Data;
using Backend.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public sealed record ServiceContext<T>(AppDbContext Db, IOptions<DatabaseLimitsOptions> DbLimits, ILogger<T> Logger, ITokenBlacklistService TokenBlacklistService, IOptions<JwtOptions> JwtOptions, JwtTokenGenerator JwtTokenGenerator) {
    public int MaxUnpagedItems => DbLimits.Value.MaxItemsLoadedWithoutPagination;
}