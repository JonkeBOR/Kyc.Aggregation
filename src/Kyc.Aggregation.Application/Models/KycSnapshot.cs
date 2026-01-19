using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Models;

public class KycSnapshot
{
    public required string Ssn { get; set; }
    public required AggregatedKycDataDto Data { get; set; }
    public DateTime FetchedAtUtc { get; set; } = DateTime.UtcNow;
}
