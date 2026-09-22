using Paperless.Domain;

namespace Paperless.Business;

// The DI container supplies these dependencies. Tests inject a mocked repository.
public sealed class DocumentService(IDocumentRepository repository, TimeProvider clock) : IDocumentService
{
    /// Returns all document metadata through the repository.
    public Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default) => repository.GetAllAsync(ct);

    /// Loads a document or reports that its ID does not exist.
    public async Task<Document> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await repository.GetByIdAsync(id, ct) ?? throw new DocumentNotFoundException(id);

    /// Validates metadata, assigns an ID and UTC timestamp, and saves the new document.
    public async Task<Document> CreateAsync(CreateDocumentCommand command, CancellationToken ct = default)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Title = RequiredText(command.Title, 200, "Title"),
            FileName = RequiredText(command.FileName, 255, "File name"),
            CreatedAt = clock.GetUtcNow()
        };
        await repository.AddAsync(document, ct);
        return document;
    }

    /// Updates valid metadata while preserving the document ID and creation time.
    public async Task<Document> UpdateAsync(Guid id, UpdateDocumentCommand command, CancellationToken ct = default)
    {
        // Validate before changing an entity that may be tracked by EF Core.
        var title = RequiredText(command.Title, 200, "Title");
        var fileName = RequiredText(command.FileName, 255, "File name");
        var document = await GetByIdAsync(id, ct);
        document.Title = title;
        document.FileName = fileName;
        await repository.UpdateAsync(document, ct);
        return document;
    }

    /// Checks that the document exists before requesting its deletion.
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var document = await GetByIdAsync(id, ct);
        await repository.DeleteAsync(document, ct);
    }

    /// Returns notes only after confirming that their parent document exists.
    public async Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default)
    {
        await GetByIdAsync(documentId, ct);
        return await repository.GetNotesAsync(documentId, ct);
    }

    /// Validates and saves a new note linked to an existing document.
    public async Task<DocumentNote> AddNoteAsync(Guid documentId, CreateNoteCommand command, CancellationToken ct = default)
    {
        var text = RequiredText(command.Text, 2000, "Note text");
        await GetByIdAsync(documentId, ct);
        var note = new DocumentNote
        {
            Id = Guid.NewGuid(), DocumentId = documentId, Text = text, CreatedAt = clock.GetUtcNow()
        };
        await repository.AddNoteAsync(note, ct);
        return note;
    }

    /// Trims input and rejects missing text or values exceeding the field limit.
    private static string RequiredText(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > maxLength)
            throw new BusinessValidationException($"{field} must contain between 1 and {maxLength} characters.");
        return trimmed;
    }
}
