using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Paperless.Business;

namespace Paperless.Api.Errors;

// Converts known business errors into HTTP responses.
public class BusinessExceptionHandler : IExceptionHandler
{
    // Handles known errors and returns false for other errors.
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        int status;
        string title;
        // Checks whether the business input was invalid.
        if (exception is BusinessValidationException)
        {
            status = StatusCodes.Status400BadRequest;
            title = "Invalid input";
        }
        // Checks whether a document was missing.
        else if (exception is DocumentNotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            title = "Document not found";
        }
        // Leaves all other errors to the remaining error handling.
        else
        {
            // Reports that this handler did not handle the error.
            return false;
        }
        // Sets the response's HTTP status.
        context.Response.StatusCode = status;
        // Creates a standard error response object.
        ProblemDetails problem = new ProblemDetails();
        // Includes the status code in the response body.
        problem.Status = status;
        // Includes the short error title.
        problem.Title = title;
        // Includes the explanation from the business exception.
        problem.Detail = exception.Message;
        // Sends the error response as JSON.
        await context.Response.WriteAsJsonAsync(problem, cancellationToken: ct);
        // Reports that the error has been handled.
        return true;
    }
}
