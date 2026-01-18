using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Abstractions;

/// <summary>
/// Unified interface for the Customer Data API client.
/// Combines personal details, contact details, and KYC form endpoints.
/// </summary>
public interface ICustomerDataApiClient
{
    /// <summary>
    /// Gets personal details for a customer by SSN.
    /// </summary>
    Task<PersonalDetailsData?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default);
    /// <summary>
    /// Gets contact details for a customer by SSN.
    /// </summary>
    Task<ContactDetailsData?> GetContactDetailsAsync(string ssn, CancellationToken ct = default);
    /// <summary>
    /// Gets KYC form data for a customer by SSN.
    /// </summary>
    Task<KycFormData?> GetKycFormAsync(string ssn, DateTime asOfDate, CancellationToken ct = default);

}

public class ContactDetailsData
{
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

public class PersonalDetailsData
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
