namespace Kyc.Aggregation.Application.Abstractions;

/// <summary>
/// External API client for fetching contact details.
/// </summary>
public interface ICustomerContactDetailsClient
{
    /// <summary>
    /// Gets contact details for a customer by SSN.
    /// </summary>
    Task<ContactDetailsData?> GetContactDetailsAsync(string ssn, CancellationToken ct = default);
}

public class ContactDetailsData
{
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
