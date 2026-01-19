using Kyc.Aggregation.Application.Interfaces;
using Kyc.Aggregation.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Kyc.Aggregation.Application.Workflows;

namespace Kyc.Aggregation.Application;

/// <summary>
/// Dependency injection extension for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Application layer services.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Register application services
        services.AddScoped<IKycAggregationService, KycAggregationService>();
        services.AddScoped<IKycCacheSnapshotService, KycCacheSnapshotService>();
        services.AddScoped<ICustomerKycDataProvider, CustomerKycDataProvider>();
        services.AddScoped<GetAggregatedKycDataWorkflow>();
        services.AddScoped<IGetAggregatedKycDataWorkflow>(sp =>
            new CachedGetAggregatedKycDataWorkflow(
                sp.GetRequiredService<GetAggregatedKycDataWorkflow>(),
                sp.GetRequiredService<IKycCacheSnapshotService>(),
                sp.GetRequiredService<IClock>()));

        return services;
    }
}
