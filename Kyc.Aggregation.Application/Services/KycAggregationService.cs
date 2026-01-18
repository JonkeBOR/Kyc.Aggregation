using Kyc.Aggregation.Application.Abstractions.Caching;
using Kyc.Aggregation.Application.Abstractions.ExternalApis;
using Kyc.Aggregation.Application.Abstractions.Time;
using Kyc.Aggregation.Contracts;

namespace Kyc.Aggregation.Application.Services;

public class KycAggregationService
{
    private readonly ICustomerPersonalDetailsClient _personalDetailsClient;
    private readonly ICustomerContactDetailsClient _contactDetailsClient;
    private readonly IKycFormClient _kycFormClient;
    private readonly IKycHotCache _hotCache;
    private readonly IKycSnapshotStore _snapshotStore;
    private readonly IClock _clock;
    private readonly KycCachingPolicy _cachingPolicy;

    public KycAggregationService(
        ICustomerPersonalDetailsClient personalDetailsClient,
        ICustomerContactDetailsClient contactDetailsClient,
        IKycFormClient kycFormClient,
        IKycHotCache hotCache,
        IKycSnapshotStore snapshotStore,
        IClock clock,
        KycCachingPolicy cachingPolicy)
    {
        _personalDetailsClient = personalDetailsClient;
        _contactDetailsClient = contactDetailsClient;
        _kycFormClient = kycFormClient;
        _hotCache = hotCache;
        _snapshotStore = snapshotStore;
        _clock = clock;
        _cachingPolicy = cachingPolicy;
    }

    public async Task<AggregatedKycDataDto?> GetAggregatedKycDataAsync(
        string ssn,
        CancellationToken cancellationToken = default)
    {
        var cachedData = _hotCache.Get(ssn);
        if (cachedData is not null)
        {
            return cachedData;
        }

        var snapshot = await _snapshotStore.GetSnapshotAsync(ssn, cancellationToken);
        if (snapshot is not null)
        {
            _hotCache.Set(ssn, snapshot, _cachingPolicy.HotCacheTtl);
            return snapshot;
        }

        var aggregated = await FetchAndAggregateAsync(ssn, cancellationToken);
        if (aggregated is not null)
        {
            await _snapshotStore.SaveSnapshotAsync(ssn, aggregated, cancellationToken);
            _hotCache.Set(ssn, aggregated, _cachingPolicy.HotCacheTtl);
        }

        return aggregated;
    }

    private async Task<AggregatedKycDataDto?> FetchAndAggregateAsync(
        string ssn,
        CancellationToken cancellationToken)
    {
        var personalDetailsTask = _personalDetailsClient.GetPersonalDetailsAsync(ssn, cancellationToken);
        var contactDetailsTask = _contactDetailsClient.GetContactDetailsAsync(ssn, cancellationToken);
        var kycFormTask = _kycFormClient.GetKycFormAsync(ssn, _clock.UtcNow.Date, cancellationToken);

        await Task.WhenAll(personalDetailsTask, contactDetailsTask, kycFormTask);

        var personalDetails = await personalDetailsTask;
        var contactDetails = await contactDetailsTask;
        var kycForm = await kycFormTask;

        if (personalDetails is null)
        {
            return null;
        }

        var kycItems = kycForm?.Items?.ToDictionary(item => item.Key?.ToLowerInvariant() ?? "", item => item.Value) 
                       ?? new Dictionary<string, string?>();

        var address = contactDetails?.Address?.FirstOrDefault();
        var addressStr = FormatAddress(address);

        var email = contactDetails?.Emails?.FirstOrDefault(e => e.Preferred) 
                    ?? contactDetails?.Emails?.FirstOrDefault();

        var phone = contactDetails?.PhoneNumbers?.FirstOrDefault(p => p.Preferred) 
                    ?? contactDetails?.PhoneNumbers?.FirstOrDefault();

        var taxCountry = ExtractValue(kycItems, "tax_country") ?? "SE";
        var incomeStr = ExtractValue(kycItems, "annual_income");
        int? income = int.TryParse(incomeStr, out var incomeVal) ? incomeVal : null;

        return new AggregatedKycDataDto
        {
            Ssn = ssn,
            FirstName = personalDetails.FirstName ?? string.Empty,
            LastName = personalDetails.SurName ?? string.Empty,
            Address = addressStr ?? string.Empty,
            PhoneNumber = phone?.Number,
            Email = email?.EmailAddress,
            TaxCountry = taxCountry,
            Income = income
        };
    }

    private static string? FormatAddress(AddressDto? address)
    {
        if (address is null)
        {
            return null;
        }

        var parts = new[]
        {
            address.Street,
            address.PostalCode,
            address.City,
            address.Country
        }.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

        return parts.Count > 0 ? string.Join(", ", parts) : null;
    }

    private static string? ExtractValue(Dictionary<string, string?> kycItems, string key)
    {
        return kycItems.TryGetValue(key, out var value) ? value : null;
    }
}
