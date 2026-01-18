using System.Text.Json.Serialization;

namespace Kyc.Aggregation.Application.Models.ExternalApis;

/// <summary>
/// Vendor DTO from Customer Data API - Personal Details endpoint.
/// </summary>
public class PersonalDetailsResponseDto
{
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("sur_name")]
    public string? SurName { get; set; }
}
