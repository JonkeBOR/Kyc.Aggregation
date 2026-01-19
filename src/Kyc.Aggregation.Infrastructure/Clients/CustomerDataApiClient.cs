using Kyc.Aggregation.Application.Exceptions;
using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Application.Models.ExternalApis;
using Microsoft.Extensions.Logging;

namespace Kyc.Aggregation.Infrastructure.Clients;

/// <summary>
/// HTTP client for the Customer Data API.
/// Returns raw vendor DTOs - all mapping to application models is handled in the Application layer.
/// </summary>
public class CustomerDataApiClient(HttpClient httpClient, ILogger<CustomerDataApiClient> logger) : ICustomerDataApiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<CustomerDataApiClient> _logger = logger;

    private async Task<T?> GetAsync<T>(string relativePath, string warningMessage, CancellationToken ct)
    {
        try
        {
            var response = await _httpClient.GetAsync(relativePath, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(warningMessage, response.StatusCode);
                throw new ExternalDependencyException(
                    dependencyName: "CustomerDataApi",
                    message: $"Customer Data API returned non-success status code {(int)response.StatusCode} ({response.StatusCode}) for '{relativePath}'.");
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing GET {RelativePath}", relativePath);
            if (ex is ExternalDependencyException)
                throw;

            throw new ExternalDependencyException(
                dependencyName: "CustomerDataApi",
                message: $"Customer Data API request failed for '{relativePath}'.",
                innerException: ex);
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
