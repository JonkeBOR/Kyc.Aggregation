using Kyc.Aggregation.Contracts;
using Kyc.Aggregation.Application.Exceptions;
using Kyc.Aggregation.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Application.Interfaces;

namespace Kyc.Aggregation.Application.Features.GetAggregatedKycData;

/// <summary>
/// Handler for aggregating KYC data from multiple sources with caching support.
/// </summary>
public class GetAggregatedKycDataHandler(
    IKycCacheSnapshotService cacheSnapshotService,
    ICustomerKycDataProvider customerKycDataProvider,
    IKycAggregationService aggregationService,
    ILogger<GetAggregatedKycDataHandler> logger) : IRequestHandler<GetAggregatedKycDataQuery, AggregatedKycDataDto>
{
    private readonly IKycCacheSnapshotService _cacheSnapshotService = cacheSnapshotService;
    private readonly ICustomerKycDataProvider _customerKycDataProvider = customerKycDataProvider;
    private readonly IKycAggregationService _aggregationService = aggregationService;
    private readonly ILogger<GetAggregatedKycDataHandler> _logger = logger;

    public async Task<AggregatedKycDataDto> Handle(GetAggregatedKycDataQuery request, CancellationToken cancellationToken)
    {
        var ssn = request.Ssn;

        var cachedOrFreshData = await _cacheSnapshotService.TryGetCachedOrFreshSnapshotDataAsync(ssn, cancellationToken);
        if (cachedOrFreshData is not null)
            return cachedOrFreshData;

        try
        {
            var input = await _customerKycDataProvider.GetCustomerKycInputAsync(ssn, cancellationToken);
            if (input.PersonalDetails is null)
                throw new NotFoundException($"Customer not found for SSN: {ssn}");


            var aggregatedData = _aggregationService.AggregateData(
                ssn,
                input.PersonalDetails,
                input.ContactDetails,
                input.KycForm);

            var newSnapshot = new KycSnapshot
            {
                Ssn = ssn,
                Data = aggregatedData,
                FetchedAtUtc = input.RequestedAtUtc
            };

            await _cacheSnapshotService.SaveSnapshotAndUpdateHotCacheAsync(newSnapshot, cancellationToken);

            return aggregatedData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data from external APIs for SSN: {Ssn}", ssn);
            
            var staleSnapshotData = await _cacheSnapshotService.TryGetStaleSnapshotDataAsync(ssn, cancellationToken);
            if (staleSnapshotData is not null)
            {
                _logger.LogWarning("Falling back to stale snapshot for SSN: {Ssn}", ssn);
                return staleSnapshotData;
            }

            throw;
        }
    }
}
