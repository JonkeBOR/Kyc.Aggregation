using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Abstractions;

/// <summary>
/// In-memory cache for aggregated KYC data.
/// </summary>
public interface IKycHotCache
{
    /// <summary>
    /// Gets cached data for a customer by SSN.
    /// </summary>
    bool TryGetValue(string ssn, out AggregatedKycDataDto? data);

    /// <summary>
    /// Sets or updates cached data for a customer.
    /// </summary>
    void Set(string ssn, AggregatedKycDataDto data, TimeSpan absoluteExpiration);
}
