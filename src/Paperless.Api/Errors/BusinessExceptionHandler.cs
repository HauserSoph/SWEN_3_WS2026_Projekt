using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Paperless.Business;

namespace Paperless.Api.Errors;

// Business code knows no HTTP status codes. Translation belongs at the API boundary.
public sealed class BusinessExceptionHandler : IExceptionHandler
{
    /// Converts known business errors to HTTP 400 or 404; leaves other errors to the default handler.
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var status = exception switch
        {
            BusinessValidationException => StatusCodes.Status400BadRequest,
            DocumentNotFoundException => StatusCodes.Status404NotFound,
            _ => 0
        };
        if (status == 0) return false;
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status == 400 ? "Invalid input" : "Document not found",
            Detail = exception.Message
        }, cancellationToken: ct);
        return true;
    }
}
