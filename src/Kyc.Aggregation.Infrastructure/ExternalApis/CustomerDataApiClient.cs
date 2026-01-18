using Kyc.Aggregation.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kyc.Aggregation.Infrastructure.ExternalApis;

/// <summary>
/// HTTP client for the Customer Data API.
/// Returns raw vendor DTOs - all mapping to application models is handled in the Application layer.
/// </summary>
public class CustomerDataApiClient : ICustomerDataApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerDataApiClient> _logger;

    public CustomerDataApiClient(HttpClient httpClient, ILogger<CustomerDataApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PersonalDetailsResponseDto?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/personal-details/{ssn}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch personal details for SSN {Ssn}: {StatusCode}", ssn, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return System.Text.Json.JsonSerializer.Deserialize<PersonalDetailsResponseDto>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching personal details for SSN {Ssn}", ssn);
            throw;
        }
    }

    public async Task<ContactDetailsResponseDto?> GetContactDetailsAsync(string ssn, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/contact-details/{ssn}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch contact details for SSN {Ssn}: {StatusCode}", ssn, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return System.Text.Json.JsonSerializer.Deserialize<ContactDetailsResponseDto>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contact details for SSN {Ssn}", ssn);
            throw;
        }
    }

    public async Task<KycFormResponseDto?> GetKycFormAsync(string ssn, DateTime asOfDate, CancellationToken ct = default)
    {
        try
        {
            var dateString = asOfDate.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync($"/kyc-form/{ssn}/{dateString}", ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch KYC form for SSN {Ssn}: {StatusCode}", ssn, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return System.Text.Json.JsonSerializer.Deserialize<KycFormResponseDto>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching KYC form for SSN {Ssn}", ssn);
            throw;
        }
    }
}
