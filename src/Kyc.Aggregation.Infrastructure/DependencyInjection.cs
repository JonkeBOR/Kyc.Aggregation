using Kyc.Aggregation.Application.Abstractions;
using Kyc.Aggregation.Infrastructure.Caching;
using Kyc.Aggregation.Infrastructure.ExternalApis;
using Kyc.Aggregation.Infrastructure.Persistence;
using Kyc.Aggregation.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kyc.Aggregation.Infrastructure;

/// <summary>
/// Dependency injection extension for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Infrastructure layer services including database, HTTP clients, caching, and time providers.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=kyc_aggregation.db";

        services.AddDbContext<KycDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register persistent snapshot store
        services.AddScoped<IKycSnapshotStore, EfKycSnapshotStore>();

        // Register hot cache
        services.AddMemoryCache();
        services.AddScoped<IKycHotCache, MemoryKycHotCache>();

        // Register clock
        services.AddSingleton<IClock, SystemClock>();

        // Register HTTP client for the Customer Data API
        var apiBaseUrl = configuration.GetSection("CustomerDataApi:BaseUrl").Value
            ?? "https://customerdataapi.azurewebsites.net/api";

        // Ensure BaseAddress ends with / for proper URI combining
        if (!apiBaseUrl.EndsWith("/"))
            apiBaseUrl += "/";

        var timeoutSeconds = configuration.GetSection("CustomerDataApi:TimeoutSeconds").Get<int?>() ?? 30;

        services.AddHttpClient<ICustomerDataApiClient, CustomerDataApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        });

        return services;
    }
}
