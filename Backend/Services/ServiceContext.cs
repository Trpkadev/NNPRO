using Backend.Config;
using Backend.Data;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public sealed record ServiceContext<T>(AppDbContext Db, IOptions<DatabaseLimitsOptions> Limits, ILogger<T> Logger) {
    public int MaxUnpagedItems => Limits.Value.MaxItemsLoadedWithoutPagination;
}