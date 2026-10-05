namespace Paperless.Business;

// Reports invalid input to the API error handler.
public class BusinessValidationException : Exception
{
    // Gives the error message to the standard Exception constructor.
    public BusinessValidationException(string message) : base(message)
    {
    }
}

// Reports that the requested document does not exist.
public class DocumentNotFoundException : Exception
{
    // Builds an error message containing the missing document ID.
    public DocumentNotFoundException(Guid id) : base($"Document '{id}' was not found.")
    {
    }
}
