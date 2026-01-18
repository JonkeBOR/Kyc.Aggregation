using Kyc.Aggregation.Application.Services;
using Kyc.Aggregation.Contracts;
using MediatR;

namespace Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;

public class GetAggregatedKycDataHandler : IRequestHandler<GetAggregatedKycDataQuery, AggregatedKycDataDto>
{
    private readonly KycAggregationService _aggregationService;

    public GetAggregatedKycDataHandler(KycAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    public async Task<AggregatedKycDataDto> Handle(
        GetAggregatedKycDataQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _aggregationService.GetAggregatedKycDataAsync(request.Ssn, cancellationToken);
        
        if (result is null)
        {
            throw new InvalidOperationException($"Customer data not found for SSN: {request.Ssn}");
        }

        return result;
    }
}
