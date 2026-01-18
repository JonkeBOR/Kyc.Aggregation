namespace Kyc.Aggregation.Infrastructure.ExternalApis.Models;

/// <summary>
/// Vendor DTO from Customer Data API - KYC Form endpoint.
/// </summary>
public class KycFormResponseDto
{
    public List<KycItemDto>? Items { get; set; }
}

public class KycItemDto
{
    public string? Key { get; set; }
    public string? Value { get; set; }
}
