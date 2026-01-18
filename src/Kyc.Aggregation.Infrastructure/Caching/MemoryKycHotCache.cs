using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Kyc.Aggregation.Infrastructure.Caching;

/// <summary>
/// In-memory cache implementation for hot KYC data access.
/// </summary>
public class MemoryKycHotCache(IMemoryCache cache, ILogger<MemoryKycHotCache> logger) : IKycHotCache
{
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<MemoryKycHotCache> _logger = logger;

    private const string CacheKeyPrefix = "kyc:";

    public bool TryGetValue(string ssn, out AggregatedKycDataDto? data)
    {
        var key = BuildCacheKey(ssn);
        var result = _cache.TryGetValue(key, out AggregatedKycDataDto? cachedData);

        if (result)
        {
            _logger.LogDebug("Hot cache hit for SSN {Ssn}", ssn);
        }

        data = cachedData;
        return result;
    }

    public void Set(string ssn, AggregatedKycDataDto data, TimeSpan absoluteExpiration)
    {
        var key = BuildCacheKey(ssn);
        _cache.Set(key, data, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration
        });

        _logger.LogDebug("Hot cache set for SSN {Ssn} with TTL {TtlSeconds}s", ssn, absoluteExpiration.TotalSeconds);
    }

    private static string BuildCacheKey(string ssn) => $"{CacheKeyPrefix}{ssn}";
}
