using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Kyc.Aggregation.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "KYC Aggregation Service",
                Version = "1.0.0",
                Description = "API for aggregating KYC (Know Your Customer) data"
            });
        });

        return services;
    }
}
