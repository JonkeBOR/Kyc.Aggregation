using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Interfaces;

public interface IGetAggregatedKycDataWorkflow
{
    Task<AggregatedKycDataDto> GetAsync(string ssn, CancellationToken ct = default);
}
