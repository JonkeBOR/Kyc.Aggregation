using Kyc.Aggregation.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Kyc.Aggregation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        services.AddSingleton(new KycCachingPolicy
        {
            SnapshotMaxAge = TimeSpan.FromHours(24),
            HotCacheTtl = TimeSpan.FromMinutes(10)
        });

        services.AddScoped<KycAggregationService>();

        return services;
    }
}
