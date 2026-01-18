using System.Text.Json;
using Kyc.Aggregation.Application.Abstractions.Caching;
using Kyc.Aggregation.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kyc.Aggregation.Infrastructure.Persistence;

public class EfKycSnapshotStore : IKycSnapshotStore
{
    private readonly KycDbContext _context;

    public EfKycSnapshotStore(KycDbContext context)
    {
        _context = context;
    }

    public async Task<AggregatedKycDataDto?> GetSnapshotAsync(
        string ssn,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _context.CustomerKycSnapshots
            .FirstOrDefaultAsync(x => x.Ssn == ssn, cancellationToken);

        if (snapshot is null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AggregatedKycDataDto>(snapshot.AggregatedPayload);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveSnapshotAsync(
        string ssn,
        AggregatedKycDataDto data,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(data);
        var utcNow = DateTime.UtcNow;

        var existing = await _context.CustomerKycSnapshots
            .FirstOrDefaultAsync(x => x.Ssn == ssn, cancellationToken);

        if (existing is not null)
        {
            existing.AggregatedPayload = payload;
            existing.UpdatedAtUtc = utcNow;
            _context.CustomerKycSnapshots.Update(existing);
        }
        else
        {
            var snapshot = new CustomerKycSnapshot
            {
                Ssn = ssn,
                AggregatedPayload = payload,
                FetchedAtUtc = utcNow,
                UpdatedAtUtc = utcNow
            };

            _context.CustomerKycSnapshots.Add(snapshot);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
