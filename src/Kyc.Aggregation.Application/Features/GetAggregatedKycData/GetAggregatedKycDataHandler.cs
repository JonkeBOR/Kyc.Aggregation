using Kyc.Aggregation.Contracts;
using MediatR;
using Kyc.Aggregation.Application.Interfaces;

namespace Kyc.Aggregation.Application.Features.GetAggregatedKycData;

/// <summary>
/// Handler for aggregating KYC data from multiple sources with caching support.
/// </summary>
public class GetAggregatedKycDataHandler(
    IGetAggregatedKycDataWorkflow workflow) : IRequestHandler<GetAggregatedKycDataQuery, AggregatedKycDataDto>
{
    private readonly IGetAggregatedKycDataWorkflow _workflow = workflow;

    public async Task<AggregatedKycDataDto> Handle(GetAggregatedKycDataQuery request, CancellationToken cancellationToken)
    {
        return await _workflow.GetAsync(request.Ssn, cancellationToken);
    }
}
