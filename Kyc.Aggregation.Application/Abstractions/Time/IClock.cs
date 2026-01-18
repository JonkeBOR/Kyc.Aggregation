namespace Kyc.Aggregation.Application.Abstractions.Time;

public interface IClock
{
    DateTime UtcNow { get; }
}
