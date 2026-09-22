using Paperless.Domain;

namespace Paperless.Business;

public sealed record CreateDocumentCommand(string Title, string FileName);
public sealed record UpdateDocumentCommand(string Title, string FileName);
public sealed record CreateNoteCommand(string Text);

public interface IDocumentService
{
    /// Returns the available documents as a list, which may be empty.
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default);
    /// Returns a document or throws DocumentNotFoundException for an unknown ID.
    Task<Document> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// Creates a document from validated metadata with a server-generated identity.
    Task<Document> CreateAsync(CreateDocumentCommand command, CancellationToken ct = default);
    /// Replaces editable metadata without changing identity or creation time.
    Task<Document> UpdateAsync(Guid id, UpdateDocumentCommand command, CancellationToken ct = default);
    /// Deletes an existing document; unknown IDs produce DocumentNotFoundException.
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    /// Lists notes for an existing document; unknown document IDs are rejected.
    Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default);
    /// Adds a validated note to an existing document.
    Task<DocumentNote> AddNoteAsync(Guid documentId, CreateNoteCommand command, CancellationToken ct = default);
}
