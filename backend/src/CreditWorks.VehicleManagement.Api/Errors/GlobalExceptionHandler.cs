using Microsoft.AspNetCore.Diagnostics;

namespace CreditWorks.VehicleManagement.Api.Errors;

/// <summary>
/// Catches any error an endpoint doesn't handle, logs it, and returns a short problem details response. The client
/// never sees technical details such as SQL messages or stack traces.
/// </summary>
public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int status;
        string title;

        if (exception is BadHttpRequestException)
        {
            // The request itself was broken, such as invalid JSON.
            status = StatusCodes.Status400BadRequest;
            title = "The request is not valid.";
        }
        else
        {
            logger.LogError(exception, "Unexpected error while handling {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            status = StatusCodes.Status500InternalServerError;
            title = "The server had a problem. Please try again.";
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = { Status = status, Title = title },
        });
    }
}
