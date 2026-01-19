using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Mappers;
using Kyc.Aggregation.Application.Models;

namespace Kyc.Aggregation.Application.Services;

public sealed class CustomerKycDataProvider(ICustomerDataApiClient customerDataApiClient, IClock clock)
    : ICustomerKycDataProvider
{
    private readonly ICustomerDataApiClient _customerDataApiClient = customerDataApiClient;
    private readonly IClock _clock = clock;

    public async Task<CustomerKycInput> GetCustomerKycInputAsync(string ssn, CancellationToken ct = default)
    {
        var requestedAtUtc = _clock.UtcNow;

        var personalDetailsResponse = await _customerDataApiClient.GetPersonalDetailsAsync(ssn, ct);
        var contactDetailsResponse = await _customerDataApiClient.GetContactDetailsAsync(ssn, ct);
        var kycFormResponse = await _customerDataApiClient.GetKycFormAsync(ssn, requestedAtUtc, ct);

        var personalDetails = personalDetailsResponse?.ToPersonalDetailsData();
        var contactDetails = contactDetailsResponse.ToContactDetailsData();
        var kycForm = kycFormResponse.ToKycFormData();

        return new CustomerKycInput(personalDetails, contactDetails, kycForm, requestedAtUtc);
    }
}
