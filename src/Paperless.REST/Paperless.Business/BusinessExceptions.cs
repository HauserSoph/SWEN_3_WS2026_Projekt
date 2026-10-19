namespace Paperless.Business;
public class BusinessValidationException : Exception
{
    public BusinessValidationException(string message) : base(message)
    {
    }
}
public class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(Guid id) : base($"Document '{id}' was not found.")
    {
    }
}
