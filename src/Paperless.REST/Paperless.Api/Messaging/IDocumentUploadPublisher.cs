namespace Paperless.Api.Messaging;

public interface IDocumentUploadPublisher
{
    Task PublishAsync(DocumentUploadJob job, CancellationToken cancellationToken = default);
}
