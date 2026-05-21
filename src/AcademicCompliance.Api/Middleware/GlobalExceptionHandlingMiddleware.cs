using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace AcademicCompliance.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
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
        catch (ValidationException exception)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status404NotFound,
                "Not found.",
                exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "The request could not be completed.");
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException("The response has already started.");
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
