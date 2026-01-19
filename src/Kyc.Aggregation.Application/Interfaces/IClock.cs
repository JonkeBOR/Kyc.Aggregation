namespace Kyc.Aggregation.Application.Interfaces;

/// <summary>
/// Time abstraction for testability and consistent clock behavior.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    DateTime UtcNow { get; }
}
