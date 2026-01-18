namespace Kyc.Aggregation.Application.Abstractions;

/// <summary>
/// Unified interface for the Customer Data API client.
/// Returns raw vendor DTOs - mapping to application models is handled by the Application layer.
/// </summary>
public interface ICustomerDataApiClient
{
    /// <summary>
    /// Gets personal details for a customer by SSN.
    /// </summary>
    Task<PersonalDetailsResponseDto?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default);

    /// <summary>
    /// Gets contact details for a customer by SSN.
    /// </summary>
    Task<ContactDetailsResponseDto?> GetContactDetailsAsync(string ssn, CancellationToken ct = default);

    /// <summary>
    /// Gets KYC form data for a customer by SSN.
    /// </summary>
    Task<KycFormResponseDto?> GetKycFormAsync(string ssn, DateTime asOfDate, CancellationToken ct = default);
}
