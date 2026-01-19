using Kyc.Aggregation.Contracts;
using MediatR;

namespace Kyc.Aggregation.Application.Features.GetAggregatedKycData;

/// <summary>
/// Query to retrieve aggregated KYC data for a customer.
/// </summary>
public record GetAggregatedKycDataQuery(string Ssn) : IRequest<AggregatedKycDataDto>;
