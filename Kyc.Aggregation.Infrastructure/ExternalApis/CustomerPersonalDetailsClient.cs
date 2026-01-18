using System.Net.Http.Json;
using Kyc.Aggregation.Application.Abstractions.ExternalApis;

namespace Kyc.Aggregation.Infrastructure.ExternalApis;

public class CustomerPersonalDetailsClient : ICustomerPersonalDetailsClient
{
    private readonly HttpClient _httpClient;

    public CustomerPersonalDetailsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PersonalDetailsDto?> GetPersonalDetailsAsync(
        string ssn,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/personal-details/{Uri.EscapeDataString(ssn)}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PersonalDetailsDto?>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
