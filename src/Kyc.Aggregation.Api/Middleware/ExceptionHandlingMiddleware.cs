using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, StatusCodes.Status404NotFound, "Customer data not found for the provided SSN.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, StatusCodes.Status400BadRequest, "Invalid input provided.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, StatusCodes.Status500InternalServerError, 
                "An unexpected error occurred while processing the request.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new { error = message };
        var json = JsonSerializer.Serialize(response);

        return context.Response.WriteAsync(json);
    }
}
