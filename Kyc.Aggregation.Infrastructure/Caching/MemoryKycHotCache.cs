using Kyc.Aggregation.Application.Abstractions.Caching;
using Kyc.Aggregation.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Kyc.Aggregation.Infrastructure.Caching;

public class MemoryKycHotCache : IKycHotCache
{
    private readonly IMemoryCache _memoryCache;
    private const string CacheKeyPrefix = "kyc";

    public MemoryKycHotCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public AggregatedKycDataDto? Get(string ssn)
    {
        var key = GetCacheKey(ssn);
        return _memoryCache.TryGetValue(key, out AggregatedKycDataDto? value) ? value : null;
    }

    public void Set(string ssn, AggregatedKycDataDto data, TimeSpan? ttl = null)
    {
        var key = GetCacheKey(ssn);
        var cacheOptions = new MemoryCacheEntryOptions();

        if (ttl.HasValue)
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = ttl;
        }
        else
        {
            cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        }

        _memoryCache.Set(key, data, cacheOptions);
    }

    public void Remove(string ssn)
    {
        var key = GetCacheKey(ssn);
        _memoryCache.Remove(key);
    }

    private static string GetCacheKey(string ssn) => $"{CacheKeyPrefix}:{ssn}";
}
