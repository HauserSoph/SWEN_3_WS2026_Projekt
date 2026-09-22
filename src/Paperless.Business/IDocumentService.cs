using Paperless.Domain;

namespace Paperless.Business;

// Carries the business input needed to create a document.
public class CreateDocumentCommand
{
    // Stores the title supplied to this command.
    public string Title { get; init; }

    public string FileName { get; init; }

    // Creates the command with the supplied input.
    public CreateDocumentCommand(string title, string fileName)
    {
        Title = title;
        FileName = fileName;
    }
}
// Carries the replacement title and file name for an update.
public class UpdateDocumentCommand
{
    public string Title { get; init; }

    public string FileName { get; init; }

    public UpdateDocumentCommand(string title, string fileName)
    {
        Title = title;
        FileName = fileName;
    }
}
// Carries the text needed to create a note.
public class CreateNoteCommand
{
    public string Text { get; init; }

    public CreateNoteCommand(string text)
    {
        Text = text;
    }
}

// Defines the business operations available to the API.
public interface IDocumentService
{
    // Requires an asynchronous method that returns the documents as a list.
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default);
    // Requires a document lookup that reports missing IDs with an exception.
    Task<Document> GetByIdAsync(Guid id, CancellationToken ct = default);
    // Requires a business method that creates and returns a document.
    Task<Document> CreateAsync(CreateDocumentCommand command, CancellationToken ct = default);
    // Requires a business method that updates and returns a document.
    Task<Document> UpdateAsync(Guid id, UpdateDocumentCommand command, CancellationToken ct = default);
    // Requires a business method that deletes a document by ID.
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    // Requires an asynchronous method that returns the notes for a document.
    Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default);
    // Requires a business method that creates and returns a note.
    Task<DocumentNote> AddNoteAsync(Guid documentId, CreateNoteCommand command, CancellationToken ct = default);
}
