namespace Kyc.Aggregation.Infrastructure.ExternalApis.Models;

/// <summary>
/// Vendor DTO from Customer Data API - Personal Details endpoint.
/// </summary>
public class PersonalDetailsResponseDto
{
    public string? FirstName { get; set; }
    public string? SurName { get; set; }
}
