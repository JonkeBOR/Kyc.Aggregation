using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Infrastructure.ExternalApis.Models;
using Microsoft.Extensions.Logging;
using static Kyc.Aggregation.Application.Abstractions.ICustomerDataApiClient;

namespace Kyc.Aggregation.Infrastructure.ExternalApis;

/// <summary>
/// HTTP client for the Customer Data API.
/// Implements all three endpoints: personal details, contact details, and KYC forms.
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

    public async Task<PersonalDetailsData?> GetPersonalDetailsAsync(string ssn, CancellationToken ct = default)
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
            var dto = System.Text.Json.JsonSerializer.Deserialize<PersonalDetailsResponseDto>(json);

            if (dto == null)
                return null;

            return new PersonalDetailsData
            {
                FirstName = dto.FirstName ?? throw new InvalidOperationException("FirstName is required"),
                LastName = dto.SurName ?? throw new InvalidOperationException("LastName is required")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching personal details for SSN {Ssn}", ssn);
            throw;
        }
    }

    public async Task<ContactDetailsData?> GetContactDetailsAsync(string ssn, CancellationToken ct = default)
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
            var dto = System.Text.Json.JsonSerializer.Deserialize<ContactDetailsResponseDto>(json);

            if (dto == null)
                return null;

            return new ContactDetailsData
            {
                Address = BuildAddress(dto.Address),
                Email = GetPreferredEmail(dto.Emails),
                PhoneNumber = GetPreferredPhoneNumber(dto.PhoneNumbers)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contact details for SSN {Ssn}", ssn);
            throw;
        }
    }

    public async Task<KycFormData?> GetKycFormAsync(string ssn, DateTime asOfDate, CancellationToken ct = default)
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
            var dto = System.Text.Json.JsonSerializer.Deserialize<KycFormResponseDto>(json);

            if (dto?.Items == null)
                return null;

            var items = new Dictionary<string, string>();
            foreach (var item in dto.Items)
            {
                if (item.Key != null && item.Value != null)
                {
                    items[item.Key] = item.Value;
                }
            }

            return new KycFormData { Items = items };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching KYC form for SSN {Ssn}", ssn);
            throw;
        }
    }

    private static string? BuildAddress(List<AddressDto>? addresses)
    {
        if (addresses == null || addresses.Count == 0)
            return null;

        var addr = addresses[0];
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(addr.Street))
            parts.Add(addr.Street);
        if (!string.IsNullOrEmpty(addr.PostalCode))
            parts.Add(addr.PostalCode);
        if (!string.IsNullOrEmpty(addr.City))
            parts.Add(addr.City);

        return string.Join(", ", parts);
    }

    private static string? GetPreferredEmail(List<EmailDto>? emails)
    {
        if (emails == null || emails.Count == 0)
            return null;

        var preferred = emails.FirstOrDefault(e => e.Preferred);
        return preferred?.EmailAddress ?? emails[0].EmailAddress;
    }

    private static string? GetPreferredPhoneNumber(List<PhoneNumberDto>? phoneNumbers)
    {
        if (phoneNumbers == null || phoneNumbers.Count == 0)
            return null;

        var preferred = phoneNumbers.FirstOrDefault(p => p.Preferred);
        return preferred?.Number ?? phoneNumbers[0].Number;
    }
}
