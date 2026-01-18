using Kyc.Aggregation.Application.Abstractions.Caching;
using Kyc.Aggregation.Application.Abstractions.ExternalApis;
using Kyc.Aggregation.Application.Abstractions.Time;
using Kyc.Aggregation.Infrastructure.Caching;
using Kyc.Aggregation.Infrastructure.ExternalApis;
using Kyc.Aggregation.Infrastructure.Persistence;
using Kyc.Aggregation.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kyc.Aggregation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Server=(localdb)\\mssqllocaldb;Database=KycAggregation;Trusted_Connection=true;";

        services.AddDbContext<KycDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IKycSnapshotStore, EfKycSnapshotStore>();

        services.AddMemoryCache();
        services.AddSingleton<IKycHotCache, MemoryKycHotCache>();

        services.AddSingleton<IClock, SystemClock>();

        services.AddHttpClient<ICustomerPersonalDetailsClient, CustomerPersonalDetailsClient>(client =>
        {
            client.BaseAddress = new Uri("https://customerdataapi.azurewebsites.net/api");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<ICustomerContactDetailsClient, CustomerContactDetailsClient>(client =>
        {
            client.BaseAddress = new Uri("https://customerdataapi.azurewebsites.net/api");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<IKycFormClient, KycFormClient>(client =>
        {
            client.BaseAddress = new Uri("https://customerdataapi.azurewebsites.net/api");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
