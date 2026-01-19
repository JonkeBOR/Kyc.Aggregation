using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Interfaces;

public interface IKycCacheSnapshotService
{
    Task<AggregatedKycDataDto?> TryGetCachedOrFreshSnapshotDataAsync(string ssn, CancellationToken ct = default);

    Task SaveSnapshotAndUpdateHotCacheAsync(KycSnapshot snapshot, CancellationToken ct = default);
}
