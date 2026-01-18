namespace Kyc.Aggregation.Application.Services;

public class KycCachingPolicy
{
    public TimeSpan SnapshotMaxAge { get; set; } = TimeSpan.FromHours(24);

    public TimeSpan HotCacheTtl { get; set; } = TimeSpan.FromMinutes(10);
}
