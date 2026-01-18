using Kyc.Aggregation.Application.Abstractions.Time;

namespace Kyc.Aggregation.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
