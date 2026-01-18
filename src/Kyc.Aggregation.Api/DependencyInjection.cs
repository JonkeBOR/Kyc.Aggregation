namespace Kyc.Aggregation.Api;

/// <summary>
/// Dependency injection extension for the API layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers API layer services including controllers, Swagger, and API-specific configuration.
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }
}
