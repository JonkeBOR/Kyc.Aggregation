using System.Net.Http.Json;
using Kyc.Aggregation.Application.Abstractions.ExternalApis;

namespace Kyc.Aggregation.Infrastructure.ExternalApis;

public class KycFormClient : IKycFormClient
{
    private readonly HttpClient _httpClient;

    public KycFormClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<KycFormDto?> GetKycFormAsync(
        string ssn,
        DateTime asOfDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dateStr = asOfDate.ToString("yyyy-MM-dd");
            var response = await _httpClient.GetAsync(
                $"/kyc-form/{Uri.EscapeDataString(ssn)}/{dateStr}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<KycFormDto?>(cancellationToken: cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
