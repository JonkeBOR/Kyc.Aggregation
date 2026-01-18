using Kyc.Aggregation.Api.Middleware;

namespace Kyc.Aggregation.Api.Extensions;

/// <summary>
/// Extension methods for configuring the API pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the API pipeline with middleware and routing.
    /// </summary>
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        // Add exception handling middleware early in the pipeline
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        return app;
    }
}
