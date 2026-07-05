using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WikiGuessrAPI.Models.Exceptions;

namespace WikiGuessrAPI;

public class ExceptionHandler : IExceptionHandler
{
    private static readonly List<Type> NotFoundExceptions =
    [
        typeof(SessionNotFoundException),
        typeof(PlayerNotInSessionException),
    ];

    private static readonly List<Type> ForbiddenExceptions =
    [
        typeof(PlayerNotHostException),
    ];

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            _ when NotFoundExceptions.Contains(exception.GetType())
                => (StatusCodes.Status404NotFound, "Resource Not Found"),

            _ when ForbiddenExceptions.Contains(exception.GetType())
                => (StatusCodes.Status403Forbidden, "Forbidden"),

            _ => (0, null),
        };

        if (statusCode == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
