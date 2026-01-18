using System.Text.Json.Serialization;

namespace Kyc.Aggregation.Application.Models.ExternalApis;

/// <summary>
/// Vendor DTO from Customer Data API - Contact Details endpoint.
/// </summary>
public class ContactDetailsResponseDto
{
    [JsonPropertyName("addresses")]
    public List<AddressDto>? Addresses { get; set; }

    [JsonPropertyName("emails")]
    public List<EmailDto>? Emails { get; set; }

    [JsonPropertyName("phone_numbers")]
    public List<PhoneNumberDto>? PhoneNumbers { get; set; }
}

public class AddressDto
{
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

public class EmailDto
{
    [JsonPropertyName("preferred")]
    public bool Preferred { get; set; }

    [JsonPropertyName("email_address")]
    public string? EmailAddress { get; set; }
}

public class PhoneNumberDto
{
    [JsonPropertyName("preferred")]
    public bool Preferred { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }
}
