using Kyc.Aggregation.Application.Exceptions;
using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Workflows;

public sealed class GetAggregatedKycDataWorkflow(
    ICustomerKycDataProvider customerKycDataProvider,
    IKycAggregationService aggregationService)
    : IGetAggregatedKycDataWorkflow
{
    private readonly ICustomerKycDataProvider _customerKycDataProvider = customerKycDataProvider;
    private readonly IKycAggregationService _aggregationService = aggregationService;

    public async Task<AggregatedKycDataDto> GetAsync(string ssn, CancellationToken ct = default)
    {
        var input = await _customerKycDataProvider.GetCustomerKycInputAsync(ssn, ct);

        if (input.PersonalDetails is null)
            throw new NotFoundException($"Customer not found for SSN: {ssn}");

        var aggregatedData = _aggregationService.AggregateData(
            ssn,
            input.PersonalDetails,
            input.ContactDetails,
            input.KycForm);

        return aggregatedData;
    }
}
