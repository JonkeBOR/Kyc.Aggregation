using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Interfaces;

/// <summary>
/// In-memory cache for aggregated KYC data.
/// </summary>
public interface IKycHotCache
{
    /// <summary>
    /// Gets cached data for a customer by SSN.
    /// </summary>
    bool TryGetValue(string ssn, out KycSnapshot? snapshot);

    /// <summary>
    /// Sets or updates cached data for a customer.
    /// </summary>
    void Set(string ssn, KycSnapshot snapshot, TimeSpan absoluteExpiration);
}
