using Kyc.Aggregation.Contracts;
using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Exceptions;
using Kyc.Aggregation.Application.Mappers;
using Kyc.Aggregation.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;

/// <summary>
/// Handler for aggregating KYC data from multiple sources with caching support.
/// </summary>
public class GetAggregatedKycDataHandler(
    IKycCacheSnapshotService cacheSnapshotService,
    ICustomerDataApiClient customerDataApiClient,
    IClock clock,
    IKycAggregationService aggregationService,
    ILogger<GetAggregatedKycDataHandler> logger) : IRequestHandler<GetAggregatedKycDataQuery, AggregatedKycDataDto>
{
    private readonly IKycCacheSnapshotService _cacheSnapshotService = cacheSnapshotService;
    private readonly IClock _clock = clock;
    private readonly ICustomerDataApiClient _customerDataApiClient = customerDataApiClient;
    private readonly IKycAggregationService _aggregationService = aggregationService;
    private readonly ILogger<GetAggregatedKycDataHandler> _logger = logger;

    public async Task<AggregatedKycDataDto> Handle(GetAggregatedKycDataQuery request, CancellationToken cancellationToken)
    {
        var ssn = request.Ssn;

        var cachedOrFreshData = await _cacheSnapshotService.TryGetCachedOrFreshSnapshotDataAsync(ssn, cancellationToken);
        if (cachedOrFreshData is not null)
            return cachedOrFreshData;

        var personalDetailsTask = _customerDataApiClient.GetPersonalDetailsAsync(ssn, cancellationToken);
        var contactDetailsTask = _customerDataApiClient.GetContactDetailsAsync(ssn, cancellationToken);
        var kycFormTask = _customerDataApiClient.GetKycFormAsync(ssn, _clock.UtcNow, cancellationToken);

        try
        {
            await Task.WhenAll(personalDetailsTask, contactDetailsTask, kycFormTask);
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

        var personalDetails = personalDetailsTask.Result;
        var contactDetails = contactDetailsTask.Result;
        var kycForm = kycFormTask.Result;

        if (personalDetails is null)
        {
            throw new NotFoundException($"Customer not found for SSN: {ssn}");
        }

        var mappedPersonalDetails = PersonalDetailsMapper.Map(personalDetails);
        var mappedContactDetails = ContactDetailsMapper.Map(contactDetails);
        var mappedKycForm = KycFormMapper.Map(kycForm);
        
        var aggregatedData = _aggregationService.AggregateData(ssn, mappedPersonalDetails, mappedContactDetails, mappedKycForm);
        
        var newSnapshot = new KycSnapshot
        {
            Ssn = ssn,
            Data = aggregatedData,
            FetchedAtUtc = _clock.UtcNow
        };

        await _cacheSnapshotService.SaveSnapshotAndUpdateHotCacheAsync(newSnapshot, cancellationToken);

        return aggregatedData;
    }
}
