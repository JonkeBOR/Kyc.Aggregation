using Kyc.Aggregation.Contracts;
using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Mappers;
using Kyc.Aggregation.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;

/// <summary>
/// Handler for aggregating KYC data from multiple sources with caching support.
/// </summary>
public class GetAggregatedKycDataHandler(
    IKycHotCache hotCache,
    IKycSnapshotStore snapshotStore,
    ICustomerDataApiClient customerDataApiClient,
    IClock clock,
    IKycAggregationService aggregationService,
    ILogger<GetAggregatedKycDataHandler> logger) : IRequestHandler<GetAggregatedKycDataQuery, AggregatedKycDataDto>
{
    private readonly IKycHotCache _hotCache = hotCache;
    private readonly IKycSnapshotStore _snapshotStore = snapshotStore;
    private readonly IClock _clock = clock;
    private readonly ICustomerDataApiClient _customerDataApiClient = customerDataApiClient;
    private readonly IKycAggregationService _aggregationService = aggregationService;
    private readonly ILogger<GetAggregatedKycDataHandler> _logger = logger;

    // Cache TTL configuration
    private static readonly TimeSpan HotCacheTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan SnapshotFreshnessThreshold = TimeSpan.FromDays(7);

    public async Task<AggregatedKycDataDto> Handle(GetAggregatedKycDataQuery request, CancellationToken cancellationToken)
    {
        var ssn = request.Ssn;

        if (_hotCache.TryGetValue(ssn, out var cachedSnapshot) && cachedSnapshot != null)
        {
            return cachedSnapshot.Data;
        }

        var snapshot = await _snapshotStore.GetLatestSnapshotAsync(ssn, cancellationToken);
        if (snapshot != null && IsSnapshotFresh(snapshot))
        {
            UpdateHotCache(snapshot);
            return snapshot.Data;
        }

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
            
            // Fallback to stale snapshot if available
            if (snapshot != null)
            {
                _logger.LogWarning("Falling back to stale snapshot for SSN: {Ssn}", ssn);
                UpdateHotCache(snapshot);
                return snapshot.Data;
            }

            throw;
        }

        var personalDetails = personalDetailsTask.Result;
        var contactDetails = contactDetailsTask.Result;
        var kycForm = kycFormTask.Result;

        if (personalDetails is null)
        {
            throw new InvalidOperationException($"Customer not found for SSN: {ssn}");
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
        await _snapshotStore.SaveSnapshotAsync(newSnapshot, cancellationToken);

        
        UpdateHotCache(newSnapshot);

        return aggregatedData;
    }

    private bool IsSnapshotFresh(KycSnapshot snapshot)
    {
        var age = _clock.UtcNow - snapshot.FetchedAtUtc;
        return age < SnapshotFreshnessThreshold;
    }

    private void UpdateHotCache(KycSnapshot snapshot)
    {
        _hotCache.Set(snapshot.Ssn, snapshot, HotCacheTtl);
    }
}
