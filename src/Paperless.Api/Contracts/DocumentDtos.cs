using System.ComponentModel.DataAnnotations;
namespace Paperless.Api.Contracts;

// Data received when a document is created.
public class CreateDocumentDto
{
    // The document title. It can be assigned during construction or initialization.
    // Rejects missing input and text exceeding the allowed length.
    [Required, StringLength(200)]
    public string Title { get; init; }

    [Required, StringLength(255)]
    public string FileName { get; init; }

    // This constructor receives the values used to create the object.
    public CreateDocumentDto(string title, string fileName)
    {
        // Stores the title parameter in the Title property.
        Title = title;
        // Stores the fileName parameter in the FileName property.
        FileName = fileName;
    }
}

// Data received when document metadata is updated.
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

// Document data sent back to the client.
public class DocumentDto
{
    // The document identifier. It can be assigned during construction or initialization.
    public Guid Id { get; init; }

    // The document title.
    public string Title { get; init; }

    // The document file name.
    public string FileName { get; init; }

    // The creation date and time.
    public DateTimeOffset CreatedAt { get; init; }

    public DocumentDto(Guid id, string title, string fileName, DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        FileName = fileName;
        CreatedAt = createdAt;
    }
}

// Data received when a note is created.
public class CreateNoteDto
{
    [Required, StringLength(2000)]
    public string Text { get; init; }

    public CreateNoteDto(string text)
    {
        Text = text;
    }
}

// Note data sent back to the client.
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
