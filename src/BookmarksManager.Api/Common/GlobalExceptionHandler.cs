using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookmarksManager.Api.Common;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (statusCode, problemDetails) = Map(exception, httpContext);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        return true;
    }

    private static (int StatusCode, ProblemDetails Details) Map(Exception exception, HttpContext httpContext)
    {
        switch (exception)
        {
            case NotFoundException notFound:
                return (StatusCodes.Status404NotFound, new ProblemDetails
                {
                    Title = "Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = notFound.Message,
                    Instance = httpContext.Request.Path,
                });

            case ConflictException conflict:
                return (StatusCodes.Status409Conflict, new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = conflict.Message,
                    Instance = httpContext.Request.Path,
                });

            case AppValidationException validation:
                return (StatusCodes.Status400BadRequest, new ValidationProblemDetails(validation.Errors)
                {
                    Title = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = httpContext.Request.Path,
                });

            default:
                return (StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = httpContext.Request.Path,
                });
        }
    }
}
