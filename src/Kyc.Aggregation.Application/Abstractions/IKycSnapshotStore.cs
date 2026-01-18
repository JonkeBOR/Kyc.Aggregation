using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Abstractions;

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

public class KycSnapshot
{
    public required string Ssn { get; set; }
    public required AggregatedKycDataDto Data { get; set; }
    public DateTime FetchedAtUtc { get; set; } = DateTime.UtcNow;
}
