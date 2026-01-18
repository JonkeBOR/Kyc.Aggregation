using Kyc.Aggregation.Contracts;
using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Kyc.Aggregation.Application.Features.KycAggregation.GetAggregatedKycData;

/// <summary>
/// Handler for aggregating KYC data from multiple sources with caching support.
/// </summary>
public class GetAggregatedKycDataHandler : IRequestHandler<GetAggregatedKycDataQuery, AggregatedKycDataDto>
{
    private readonly IKycHotCache _hotCache;
    private readonly IKycSnapshotStore _snapshotStore;
    private readonly ICustomerPersonalDetailsClient _personalDetailsClient;
    private readonly ICustomerContactDetailsClient _contactDetailsClient;
    private readonly IKycFormClient _kycFormClient;
    private readonly IClock _clock;
    private readonly IKycAggregationService _aggregationService;
    private readonly ILogger<GetAggregatedKycDataHandler> _logger;

    // Cache TTL configuration
    private static readonly TimeSpan HotCacheTtl = TimeSpan.FromHours(1);
    private static readonly TimeSpan SnapshotFreshnessThreshold = TimeSpan.FromDays(7);

    public GetAggregatedKycDataHandler(
        IKycHotCache hotCache,
        IKycSnapshotStore snapshotStore,
        ICustomerPersonalDetailsClient personalDetailsClient,
        ICustomerContactDetailsClient contactDetailsClient,
        IKycFormClient kycFormClient,
        IClock clock,
        IKycAggregationService aggregationService,
        ILogger<GetAggregatedKycDataHandler> logger)
    {
        _hotCache = hotCache;
        _snapshotStore = snapshotStore;
        _personalDetailsClient = personalDetailsClient;
        _contactDetailsClient = contactDetailsClient;
        _kycFormClient = kycFormClient;
        _clock = clock;
        _aggregationService = aggregationService;
        _logger = logger;
    }

    public async Task<AggregatedKycDataDto> Handle(GetAggregatedKycDataQuery request, CancellationToken cancellationToken)
    {
        var ssn = request.Ssn;
        _logger.LogInformation("Processing KYC aggregation request for SSN: {Ssn}", ssn);

        // 1. Try hot cache (in-memory)
        if (_hotCache.TryGetValue(ssn, out var cachedData) && cachedData != null)
        {
            _logger.LogInformation("KYC data found in hot cache for SSN: {Ssn}", ssn);
            return cachedData;
        }

        // 2. Try persistent snapshot
        var snapshot = await _snapshotStore.GetLatestSnapshotAsync(ssn, cancellationToken);
        if (snapshot != null && IsSnapshotFresh(snapshot))
        {
            _logger.LogInformation("KYC data found in persistent store for SSN: {Ssn}, cached at {FetchedAt}", 
                ssn, snapshot.FetchedAtUtc);
            UpdateHotCache(ssn, snapshot.Data);
            return snapshot.Data;
        }

        // 3. Fetch from external APIs in parallel
        _logger.LogInformation("Fetching KYC data from external APIs for SSN: {Ssn}", ssn);
        var personalDetailsTask = _personalDetailsClient.GetPersonalDetailsAsync(ssn, cancellationToken);
        var contactDetailsTask = _contactDetailsClient.GetContactDetailsAsync(ssn, cancellationToken);
        var kycFormTask = _kycFormClient.GetKycFormAsync(ssn, _clock.UtcNow, cancellationToken);

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
                UpdateHotCache(ssn, snapshot.Data);
                return snapshot.Data;
            }

            throw;
        }

        var personalDetails = personalDetailsTask.Result;
        var contactDetails = contactDetailsTask.Result;
        var kycForm = kycFormTask.Result;

        if (personalDetails == null)
        {
            throw new InvalidOperationException($"Customer not found for SSN: {ssn}");
        }

        // 4. Aggregate data
        var aggregatedData = _aggregationService.AggregateData(ssn, personalDetails, contactDetails, kycForm);

        // 5. Persist snapshot
        var newSnapshot = new KycSnapshot
        {
            Ssn = ssn,
            Data = aggregatedData,
            FetchedAtUtc = _clock.UtcNow
        };
        await _snapshotStore.SaveSnapshotAsync(newSnapshot, cancellationToken);

        // 6. Update hot cache
        UpdateHotCache(ssn, aggregatedData);

        _logger.LogInformation("Successfully aggregated and cached KYC data for SSN: {Ssn}", ssn);
        return aggregatedData;
    }

    private bool IsSnapshotFresh(KycSnapshot snapshot)
    {
        var age = _clock.UtcNow - snapshot.FetchedAtUtc;
        return age < SnapshotFreshnessThreshold;
    }

    private void UpdateHotCache(string ssn, AggregatedKycDataDto data)
    {
        _hotCache.Set(ssn, data, HotCacheTtl);
    }
}
