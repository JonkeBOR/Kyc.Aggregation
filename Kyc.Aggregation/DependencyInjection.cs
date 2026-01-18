using Microsoft.Extensions.DependencyInjection;

namespace Kyc.Aggregation;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();

        return services;
    }
}
