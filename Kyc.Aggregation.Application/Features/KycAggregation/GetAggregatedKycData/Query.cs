using Kyc.Aggregation.Contracts;
using MediatR;

namespace Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;

public class GetAggregatedKycDataQuery : IRequest<AggregatedKycDataDto>
{
    public required string Ssn { get; init; }
}
