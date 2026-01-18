using Microsoft.EntityFrameworkCore;

namespace Kyc.Aggregation.Infrastructure.Persistence;

public class CustomerKycSnapshot
{
    public int Id { get; set; }

    public required string Ssn { get; set; }

    public required string AggregatedPayload { get; set; }

    public DateTime FetchedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public class KycDbContext : DbContext
{
    public KycDbContext(DbContextOptions<KycDbContext> options) : base(options)
    {
    }

    public DbSet<CustomerKycSnapshot> CustomerKycSnapshots { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerKycSnapshot>()
            .HasKey(x => x.Id);

        modelBuilder.Entity<CustomerKycSnapshot>()
            .HasIndex(x => x.Ssn)
            .IsUnique();

        modelBuilder.Entity<CustomerKycSnapshot>()
            .Property(x => x.Ssn)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<CustomerKycSnapshot>()
            .Property(x => x.AggregatedPayload)
            .IsRequired();

        modelBuilder.Entity<CustomerKycSnapshot>()
            .Property(x => x.FetchedAtUtc)
            .IsRequired();

        modelBuilder.Entity<CustomerKycSnapshot>()
            .Property(x => x.UpdatedAtUtc)
            .IsRequired();
    }
}
