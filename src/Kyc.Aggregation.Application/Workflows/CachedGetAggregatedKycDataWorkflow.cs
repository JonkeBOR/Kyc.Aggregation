using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Workflows;

public sealed class CachedGetAggregatedKycDataWorkflow(
    IGetAggregatedKycDataWorkflow inner,
    IKycCacheSnapshotService cacheSnapshotService,
    IClock clock) : IGetAggregatedKycDataWorkflow
{
    private readonly IGetAggregatedKycDataWorkflow _inner = inner;
    private readonly IKycCacheSnapshotService _cacheSnapshotService = cacheSnapshotService;
    private readonly IClock _clock = clock;

    public async Task<AggregatedKycDataDto> GetAsync(string ssn, CancellationToken ct = default)
    {
        var cachedOrFreshData = await _cacheSnapshotService.TryGetCachedOrFreshSnapshotDataAsync(ssn, ct);
        if (cachedOrFreshData is not null)
            return cachedOrFreshData;

        var aggregatedData = await _inner.GetAsync(ssn, ct);

        var newSnapshot = new KycSnapshot
        {
            Ssn = ssn,
            Data = aggregatedData,
            FetchedAtUtc = _clock.UtcNow
        };

        await _cacheSnapshotService.SaveSnapshotAndUpdateHotCacheAsync(newSnapshot, ct);

        return aggregatedData;
    }
}