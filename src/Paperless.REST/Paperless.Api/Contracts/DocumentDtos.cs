using System.ComponentModel.DataAnnotations;
namespace Paperless.Api.Contracts;
public class CreateDocumentDto
{
    [Required, StringLength(200)]
    public string Title { get; init; }

    [Required, StringLength(255)]
    public string FileName { get; init; }
    public CreateDocumentDto(string title, string fileName)
    {
        Title = title;
        FileName = fileName;
    }
}
public class UpdateDocumentDto
{
    [Required, StringLength(200)]
    public string Title { get; init; }
    [Required, StringLength(255)]
    public string FileName { get; init; }

    public UpdateDocumentDto(string title, string fileName)
    {
        Title = title;
        FileName = fileName;
    }
}
public class DocumentDto
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string FileName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public DocumentDto(Guid id, string title, string fileName, DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        FileName = fileName;
        CreatedAt = createdAt;
    }
}
public class CreateNoteDto
{
    [Required, StringLength(2000)]
    public string Text { get; init; }
    public CreateNoteDto(string text)
    {
        Text = text;
    }
}
public class DocumentNoteDto
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public string Text { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DocumentNoteDto(Guid id, Guid documentId, string text, DateTimeOffset createdAt)
    {
        Id = id;
        DocumentId = documentId;
        Text = text;
        CreatedAt = createdAt;
    }
}
