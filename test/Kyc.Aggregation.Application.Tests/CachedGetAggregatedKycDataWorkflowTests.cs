using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Application.Workflows;
using Kyc.Aggregation.Contracts;
using Moq;

namespace Kyc.Aggregation.Application.Tests;

public sealed class CachedGetAggregatedKycDataWorkflowTests
{
    private static AggregatedKycDataDto CreateDto(string ssn) => new()
    {
        Ssn = ssn,
        FirstName = "A",
        LastName = "B",
        Address = "Street 1",
        TaxCountry = "SE"
    };

    [Fact]
    public async Task GetAsync_ReturnsCachedData_WhenAvailable()
    {
        var ssn = "19121212-1212";
        var cached = CreateDto(ssn);

        var inner = new Mock<IGetAggregatedKycDataWorkflow>(MockBehavior.Strict);
        var cache = new Mock<IKycCacheSnapshotService>(MockBehavior.Strict);
        var clock = new Mock<IClock>(MockBehavior.Strict);

        cache.Setup(x => x.TryGetCachedOrFreshSnapshotDataAsync(ssn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var sut = new CachedGetAggregatedKycDataWorkflow(inner.Object, cache.Object, clock.Object);

        var result = await sut.GetAsync(ssn, CancellationToken.None);

        Assert.Same(cached, result);
        inner.VerifyNoOtherCalls();
        cache.VerifyAll();
        clock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAsync_DelegatesToInner_WhenCacheMiss()
    {
        var ssn = "19121212-1212";
        var resultDto = CreateDto(ssn);
        var now = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        var inner = new Mock<IGetAggregatedKycDataWorkflow>(MockBehavior.Strict);
        var cache = new Mock<IKycCacheSnapshotService>(MockBehavior.Strict);
        var clock = new Mock<IClock>(MockBehavior.Strict);

        cache.Setup(x => x.TryGetCachedOrFreshSnapshotDataAsync(ssn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AggregatedKycDataDto?)null);

        inner.Setup(x => x.GetAsync(ssn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        clock.SetupGet(x => x.UtcNow).Returns(now);
        cache.Setup(x => x.SaveSnapshotAndUpdateHotCacheAsync(
                It.Is<Kyc.Aggregation.Application.Models.KycSnapshot>(s =>
                    s.Ssn == ssn &&
                    ReferenceEquals(s.Data, resultDto) &&
                    s.FetchedAtUtc == now),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new CachedGetAggregatedKycDataWorkflow(inner.Object, cache.Object, clock.Object);

        var result = await sut.GetAsync(ssn, CancellationToken.None);

        Assert.Same(resultDto, result);
        cache.VerifyAll();
        inner.VerifyAll();
        clock.VerifyAll();
    }
}
