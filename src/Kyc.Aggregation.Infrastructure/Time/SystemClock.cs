using Kyc.Aggregation.Application.Abstractions;

namespace Kyc.Aggregation.Infrastructure.Time;

/// <summary>
/// System clock implementation.
/// </summary>
public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
