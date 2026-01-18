using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Models.ExternalApis;
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

    private async Task<T?> GetAsync<T>(string relativePath, string warningMessage, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync(relativePath, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(warningMessage, response.StatusCode);
                return default;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing GET {RelativePath}", relativePath);
            throw;
        }
    }

    public async Task<PersonalDetailsResponseDto?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default)
    {
        return await GetAsync<PersonalDetailsResponseDto>(
            $"personal-details/{ssn}",
            $"Failed to fetch personal details for SSN {ssn}: {{StatusCode}}",
            ct);
    }

    public async Task<ContactDetailsResponseDto?> GetContactDetailsAsync(string ssn, CancellationToken ct = default)
    {
        return await GetAsync<ContactDetailsResponseDto>(
            $"contact-details/{ssn}",
            $"Failed to fetch contact details for SSN {ssn}: {{StatusCode}}",
            ct);
    }

    public async Task<KycFormResponseDto?> GetKycFormAsync(string ssn, DateTime asOfDate, CancellationToken ct = default)
    {
        var dateString = asOfDate.ToString("yyyy-MM-dd");
        return await GetAsync<KycFormResponseDto>(
            $"kyc-form/{ssn}/{dateString}",
            $"Failed to fetch KYC form for SSN {ssn}: {{StatusCode}}",
            ct);
    }
}
