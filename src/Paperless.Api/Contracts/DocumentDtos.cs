using System.ComponentModel.DataAnnotations;

namespace Paperless.Api.Contracts;

// Validation attributes on positional record parameters are read by ASP.NET Core.
public sealed record CreateDocumentDto(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(255)] string FileName);

public sealed record UpdateDocumentDto(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(255)] string FileName);

public sealed record DocumentDto(Guid Id, string Title, string FileName, DateTimeOffset CreatedAt);

public sealed record CreateNoteDto([Required, StringLength(2000)] string Text);

public sealed record DocumentNoteDto(Guid Id, Guid DocumentId, string Text, DateTimeOffset CreatedAt);
