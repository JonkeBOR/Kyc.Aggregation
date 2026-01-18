using Kyc.Aggregation.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Kyc.Aggregation.Infrastructure.Persistence;

/// <summary>
/// Entity Framework DbContext for KYC aggregation persistent storage.
/// </summary>
public class KycDbContext : DbContext
{
    public KycDbContext(DbContextOptions<KycDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerKycSnapshotEntity> CustomerKycSnapshots { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure CustomerKycSnapshotEntity
        modelBuilder.Entity<CustomerKycSnapshotEntity>()
            .HasKey(x => x.Ssn);

        modelBuilder.Entity<CustomerKycSnapshotEntity>()
            .Property(x => x.Ssn)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<CustomerKycSnapshotEntity>()
            .Property(x => x.DataJson)
            .IsRequired();

        modelBuilder.Entity<CustomerKycSnapshotEntity>()
            .Property(x => x.FetchedAtUtc)
            .IsRequired();

        modelBuilder.Entity<CustomerKycSnapshotEntity>()
            .HasIndex(x => x.FetchedAtUtc);
    }
}

/// <summary>
/// Entity for storing KYC data snapshots.
/// </summary>
public class CustomerKycSnapshotEntity
{
    /// <summary>
    /// Customer's Social Security Number (primary key).
    /// </summary>
    public required string Ssn { get; set; }

    /// <summary>
    /// Serialized aggregated KYC data as JSON.
    /// </summary>
    public required string DataJson { get; set; }

    /// <summary>
    /// UTC timestamp when the data was fetched and stored.
    /// </summary>
    public DateTime FetchedAtUtc { get; set; } = DateTime.UtcNow;
}
