using System.Net.Http.Json;
using Kyc.Aggregation.Application.Abstractions.ExternalApis;

namespace Kyc.Aggregation.Infrastructure.ExternalApis;

public class CustomerContactDetailsClient : ICustomerContactDetailsClient
{
    private readonly HttpClient _httpClient;

    public CustomerContactDetailsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ContactDetailsDto?> GetContactDetailsAsync(
        string ssn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/contact-details/{Uri.EscapeDataString(ssn)}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ContactDetailsDto?>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
