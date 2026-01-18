using Kyc.Aggregation.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Kyc.Aggregation.Api.Middleware;

/// <summary>
/// Global exception handling middleware for the API.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Not found: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status400BadRequest, "Bad Request", "Invalid input provided.");
        }
        catch (ExternalDependencyException ex)
        {
            _logger.LogError(ex, "External dependency failure ({DependencyName}): {Message}", ex.DependencyName, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status503ServiceUnavailable, "Service Unavailable",
                "A required upstream service is unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError, "Internal Server Error",
                "An unexpected error occurred while processing the request.");
        }
    }

    private static Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
            return Task.CompletedTask;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var traceId = context.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = traceId;

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
