namespace Kyc.Aggregation.Infrastructure.ExternalApis.Models;

/// <summary>
/// Vendor DTO from Customer Data API - Contact Details endpoint.
/// </summary>
public class ContactDetailsResponseDto
{
    public List<AddressDto>? Address { get; set; }
    public List<EmailDto>? Emails { get; set; }
    public List<PhoneNumberDto>? PhoneNumbers { get; set; }
}

public class AddressDto
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

public class EmailDto
{
    public bool Preferred { get; set; }
    public string? EmailAddress { get; set; }
}

public class PhoneNumberDto
{
    public bool Preferred { get; set; }
    public string? Number { get; set; }
}
