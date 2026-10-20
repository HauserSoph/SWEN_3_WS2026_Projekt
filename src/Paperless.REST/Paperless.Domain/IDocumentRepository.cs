namespace Paperless.Domain;
// Der Service kennt nur diesen Vertrag. Im Betrieb verwenden wir PostgreSQL,
// in Business-Tests können wir stattdessen ein Mock-Repository übergeben.
public interface IDocumentRepository
{
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Document document, CancellationToken ct = default);
    Task UpdateAsync(Document document, CancellationToken ct = default);
    Task DeleteAsync(Document document, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default);
    Task AddNoteAsync(DocumentNote note, CancellationToken ct = default);
}
