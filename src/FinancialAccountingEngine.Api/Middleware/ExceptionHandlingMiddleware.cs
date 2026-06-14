using Microsoft.AspNetCore.Mvc;

namespace FinancialAccountingEngine.Api.Middleware;

/// <summary>
/// Catches unhandled exceptions and returns a consistent <see cref="ProblemDetails"/> payload,
/// while logging the full exception server-side.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}.",
                context.Request.Method, context.Request.Path);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "The request could not be processed. Please try again later.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
