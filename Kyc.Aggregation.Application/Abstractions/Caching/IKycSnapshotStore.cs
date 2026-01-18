using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Abstractions.Caching;

public interface IKycSnapshotStore
{
    Task<AggregatedKycDataDto?> GetSnapshotAsync(string ssn, CancellationToken cancellationToken = default);

    Task SaveSnapshotAsync(string ssn, AggregatedKycDataDto data, CancellationToken cancellationToken = default);
}
