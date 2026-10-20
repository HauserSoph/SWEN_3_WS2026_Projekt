namespace Paperless.Api.Messaging;

public sealed record DocumentUploadJob(
    int Version,
    Guid DocumentId,
    string FileName,
    string ContentType,
    long Length,
    string StagedFilePath);
