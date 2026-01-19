using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Services;

public class KycCacheSnapshotService(IKycHotCache hotCache, IKycSnapshotStore snapshotStore, IClock clock)
    : IKycCacheSnapshotService
{
    private readonly IKycHotCache _hotCache = hotCache;
    private readonly IKycSnapshotStore _snapshotStore = snapshotStore;
    private readonly IClock _clock = clock;

    private static readonly TimeSpan HotCacheTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan SnapshotFreshnessThreshold = TimeSpan.FromDays(7);

    public async Task<AggregatedKycDataDto?> TryGetCachedOrFreshSnapshotDataAsync(string ssn, CancellationToken ct = default)
    {
        if (_hotCache.TryGetValue(ssn, out var cachedSnapshot) && cachedSnapshot is not null)
        {
            return cachedSnapshot.Data;
        }

        var snapshot = await _snapshotStore.GetLatestSnapshotAsync(ssn, ct);
        if (snapshot is not null && IsSnapshotFresh(snapshot))
        {
            UpdateHotCache(snapshot);
            return snapshot.Data;

        }

        return null;
    }

    public async Task SaveSnapshotAndUpdateHotCacheAsync(KycSnapshot snapshot, CancellationToken ct = default)
    {
        await _snapshotStore.SaveSnapshotAsync(snapshot, ct);
        UpdateHotCache(snapshot);
    }

    private bool IsSnapshotFresh(KycSnapshot snapshot)
    {
        var age = _clock.UtcNow - snapshot.FetchedAtUtc;
        return age < SnapshotFreshnessThreshold;
    }

    private void UpdateHotCache(KycSnapshot snapshot)
    {
        _hotCache.Set(snapshot.Ssn, snapshot, HotCacheTtl);
    }
}
