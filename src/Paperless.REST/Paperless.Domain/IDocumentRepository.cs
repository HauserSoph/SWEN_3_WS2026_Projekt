namespace Paperless.Domain;

// Defines the data-access contract without depending on database code.
public interface IDocumentRepository
{
    // Requires an asynchronous method that returns the documents as a list.
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default);
    // Requires a document lookup that returns null when the ID is missing.
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    // Requires the repository to save a new document before completing.
    Task AddAsync(Document document, CancellationToken ct = default);
    // Requires the repository to save document changes before completing.
    Task UpdateAsync(Document document, CancellationToken ct = default);
    // Requires the repository to delete the document and its notes.
    Task DeleteAsync(Document document, CancellationToken ct = default);
    // Requires an asynchronous method that returns the notes for a document.
    Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default);
    // Requires the repository to save a new note before completing.
    Task AddNoteAsync(DocumentNote note, CancellationToken ct = default);
}
