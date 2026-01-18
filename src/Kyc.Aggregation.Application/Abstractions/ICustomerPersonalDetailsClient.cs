namespace Kyc.Aggregation.Application.Abstractions;

/// <summary>
/// External API client for fetching personal details.
/// </summary>
public interface ICustomerPersonalDetailsClient
{
    /// <summary>
    /// Gets personal details for a customer by SSN.
    /// </summary>
    Task<PersonalDetailsData?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default);
}

public class PersonalDetailsData
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
