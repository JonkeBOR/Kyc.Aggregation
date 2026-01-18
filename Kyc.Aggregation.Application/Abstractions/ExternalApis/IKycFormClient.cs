namespace Kyc.Aggregation.Application.Abstractions.ExternalApis;

public class KycFormDto
{
    public List<KycItemDto>? Items { get; set; }
}

public class KycItemDto
{
    public string? Key { get; set; }

    public string? Value { get; set; }
}

public interface IKycFormClient
{
    Task<KycFormDto?> GetKycFormAsync(string ssn, DateTime asOfDate, CancellationToken cancellationToken = default);
}
