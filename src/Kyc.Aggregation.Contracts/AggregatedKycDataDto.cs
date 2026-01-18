namespace Kyc.Aggregation.Contracts;

/// <summary>
/// Aggregated KYC data for a customer.
/// Matches the OpenAPI specification response contract.
/// </summary>
public class AggregatedKycDataDto
{
    /// <summary>
    /// Customer's Social Security Number.
    /// </summary>
    public required string Ssn { get; set; }

    /// <summary>
    /// Customer's first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Customer's last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Customer's address.
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// Customer's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Customer's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Tax country code (e.g., SE, DK).
    /// </summary>
    public required string TaxCountry { get; set; }

    /// <summary>
    /// Annual income in the customer's tax country currency.
    /// </summary>
    public int? Income { get; set; }
}
