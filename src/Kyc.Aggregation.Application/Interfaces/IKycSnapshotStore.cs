using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Interfaces;

/// <summary>
/// Persistent storage for KYC data snapshots.
/// </summary>
public interface IKycSnapshotStore
{
    /// <summary>
    /// Gets the most recent snapshot for a customer by SSN.
    /// </summary>
    Task<KycSnapshot?> GetLatestSnapshotAsync(string ssn, CancellationToken ct = default);

    /// <summary>
    /// Saves or updates a snapshot for a customer.
    /// </summary>
    Task SaveSnapshotAsync(KycSnapshot snapshot, CancellationToken ct = default);
}

