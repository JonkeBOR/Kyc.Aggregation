using System.Text.Json.Serialization;

namespace Kyc.Aggregation.Application.Models.ExternalApis;

/// <summary>
/// Vendor DTO from Customer Data API - KYC Form endpoint.
/// </summary>
public class KycFormResponseDto
{
    [JsonPropertyName("items")]
    public List<KycItemDto>? Items { get; set; }
}

public class KycItemDto
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
