namespace Paperless.Business;

public sealed class BusinessValidationException(string message) : Exception(message);
public sealed class DocumentNotFoundException(Guid id) : Exception($"Document '{id}' was not found.");
