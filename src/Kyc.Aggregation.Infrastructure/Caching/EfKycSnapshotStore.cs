using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Application.Models;
using Kyc.Aggregation.Contracts;
using Kyc.Aggregation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kyc.Aggregation.Infrastructure.Caching;

/// <summary>
/// Entity Framework-backed persistent snapshot store for KYC data.
/// </summary>
public class EfKycSnapshotStore(KycDbContext dbContext, ILogger<EfKycSnapshotStore> logger) : IKycSnapshotStore
{
    private readonly KycDbContext _dbContext = dbContext;
    private readonly ILogger<EfKycSnapshotStore> _logger = logger;

    public async Task<KycSnapshot?> GetLatestSnapshotAsync(string ssn, CancellationToken ct = default)
    {
        try
        {
            var entity = await _dbContext.CustomerKycSnapshots
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Ssn == ssn, ct);

            if (entity == null)
            {
                _logger.LogDebug("No snapshot found for SSN {Ssn}", ssn);
                return null;
            }

            var data = JsonSerializer.Deserialize<AggregatedKycDataDto>(entity.DataJson);
            if (data == null)
            {
                _logger.LogWarning("Failed to deserialize snapshot data for SSN {Ssn}", ssn);
                return null;
            }

            return new KycSnapshot
            {
                Ssn = entity.Ssn,
                Data = data,
                FetchedAtUtc = entity.FetchedAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snapshot for SSN {Ssn}", ssn);
            throw;
        }
    }

    public async Task SaveSnapshotAsync(KycSnapshot snapshot, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(snapshot.Data);

        var existing = await _dbContext.CustomerKycSnapshots
            .SingleOrDefaultAsync(x => x.Ssn == snapshot.Ssn, ct);

        if (existing is null)
        {
            _dbContext.CustomerKycSnapshots.Add(new CustomerKycSnapshotEntity
            {
                Ssn = snapshot.Ssn,
                DataJson = json,
                FetchedAtUtc = snapshot.FetchedAtUtc
            });
        }
        else
        {
            existing.DataJson = json;
            existing.FetchedAtUtc = snapshot.FetchedAtUtc;
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
