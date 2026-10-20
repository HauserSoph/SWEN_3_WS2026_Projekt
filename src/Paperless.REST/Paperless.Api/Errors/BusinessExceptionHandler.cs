using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Paperless.Business;

namespace Paperless.Api.Errors;
public class BusinessExceptionHandler : IExceptionHandler
{
    // Der eingebaute Logger wird durch Dependency Injection übergeben.
    private readonly ILogger<BusinessExceptionHandler> logger;

    public BusinessExceptionHandler(ILogger<BusinessExceptionHandler> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        int status;
        string title;
        // Die Business-Schicht kennt keine HTTP-Statuscodes. Erst hier werden daraus 400 oder 404.
        if (exception is BusinessValidationException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "Invalid input";
        }
        else if (exception is DocumentNotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            title = "Document not found";
        }
        else
        {
            logger.LogError(exception, "Error: API request failed");
            // Unbekannte Fehler an die allgemeine Fehlerbehandlung weitergeben.
            // Interne Fehlerdetails sollen nicht als Business-Fehlermeldung beim Client landen.
            return false;
        }
        logger.LogWarning("Warning: {Message}", exception.Message);
        context.Response.StatusCode = status;
        ProblemDetails problem = new ProblemDetails();
        problem.Status = status;
        problem.Title = title;
        problem.Detail = exception.Message;
        await context.Response.WriteAsJsonAsync(problem, cancellationToken: ct);
        return true;
    }
}
