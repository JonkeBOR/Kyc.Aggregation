using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Abstractions;

/// <summary>
/// External API client for fetching KYC form data.
/// </summary>
public interface IKycFormClient
{
    /// <summary>
    /// Gets KYC form data for a customer by SSN as of a specific date.
    /// </summary>
    Task<KycFormData?> GetKycFormAsync(string ssn, DateTime asOfDate, CancellationToken ct = default);
}
