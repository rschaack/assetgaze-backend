// In: src/Assetgaze/Middleware/ErrorHandlingMiddleware.cs
using System.Net;
using System.Text.Json;

namespace Assetgaze.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env; // Added to check environment

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env) // Inject IHostEnvironment
    {
        _next = next;
        _logger = logger;
        _env = env; // Assign
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex, _env.IsDevelopment()); // Pass IsDevelopment flag
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = "An internal server error has occurred.",
            // Include details only in development mode for security reasons
            detail = isDevelopment ? exception.Message : null 
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}