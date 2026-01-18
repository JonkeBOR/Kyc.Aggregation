using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Abstractions.Caching;

public interface IKycHotCache
{
    AggregatedKycDataDto? Get(string ssn);

    void Set(string ssn, AggregatedKycDataDto data, TimeSpan? ttl = null);

    void Remove(string ssn);
}
