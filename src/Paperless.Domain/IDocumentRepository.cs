namespace Paperless.Domain;

// Shared contract: neither business code nor callers need to know EF Core.
public interface IDocumentRepository
{
    /// Loads all stored documents; returns an empty list when none exist.
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default);
    /// Loads a document by ID, returning null when it does not exist.
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// Persists a new document before completing.
    Task AddAsync(Document document, CancellationToken ct = default);
    /// Persists changes to an existing document before completing.
    Task UpdateAsync(Document document, CancellationToken ct = default);
    /// Deletes the document and its associated notes before completing.
    Task DeleteAsync(Document document, CancellationToken ct = default);
    /// Loads notes belonging to the specified document; may return an empty list.
    Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default);
    /// Persists a new note linked to its parent document before completing.
    Task AddNoteAsync(DocumentNote note, CancellationToken ct = default);
}
