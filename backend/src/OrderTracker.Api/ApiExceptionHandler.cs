using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderTracker.Api.Services;

namespace OrderTracker.Api;

/// <summary>Turns known business errors into ProblemDetails HTTP responses.</summary>
public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail, errors) = exception switch
        {
            OrderValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Request is invalid",
                ex.Message,
                (IReadOnlyList<string>?)ex.Errors),
            BadStatusChangeException ex => (
                StatusCodes.Status409Conflict,
                "Status change not allowed",
                ex.Message,
                null),
            OrderNotFoundException ex => (
                StatusCodes.Status404NotFound,
                "Food order not found",
                ex.Message,
                null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "Something went wrong on the server.",
                null)
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        if (errors is { Count: > 0 })
            problem.Extensions["errors"] = errors;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
