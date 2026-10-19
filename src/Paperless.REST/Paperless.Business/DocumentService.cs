using Microsoft.Extensions.Logging;
using Paperless.Domain;

namespace Paperless.Business;
public class DocumentService : IDocumentService
{
    // Der eingebaute Logger wird durch Dependency Injection übergeben.
    private readonly ILogger<DocumentService> logger;

    private readonly IDocumentRepository repository;
    private readonly TimeProvider clock;
    public DocumentService(IDocumentRepository repository, TimeProvider clock, ILogger<DocumentService> logger)
    {
        this.logger = logger;
        this.repository = repository;
        this.clock = clock;
    }
    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Document> documents = await repository.GetAllAsync(ct);
        logger.LogInformation("Business: Loaded {DocumentCount} documents.", documents.Count);
        return documents;
    }
    public async Task<Document> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessValidationException("Document ID must not be empty.");
        }
        Document? document = await repository.GetByIdAsync(id, ct);
        if (document == null)
        {
            throw new DocumentNotFoundException(id);
        }
        logger.LogInformation("Business: Loaded document {DocumentId}.", id);
        return document;
    }
    public async Task<Document> CreateAsync(CreateDocumentCommand command, CancellationToken ct = default)
    {
        if (command == null)
        {
            throw new BusinessValidationException("Document input is required.");
        }
        Document document = new Document();
        document.Id = Guid.NewGuid();
        document.Title = RequiredText(command.Title, 200, "Title");
        document.FileName = RequiredText(command.FileName, 255, "File name");
        // Die Uhr wird übergeben, damit Tests einen festen Zeitpunkt verwenden können.
        // UTC verhindert, dass die gespeicherte Zeit von der Zeitzone des Servers abhängt.
        document.CreatedAt = clock.GetUtcNow();
        await repository.AddAsync(document, ct);
        logger.LogInformation("Business: Created document {DocumentId}.", document.Id);
        return document;
    }
    public async Task<Document> UpdateAsync(Guid id, UpdateDocumentCommand command, CancellationToken ct = default)
    {
        if (command == null)
        {
            throw new BusinessValidationException("Document input is required.");
        }
        // Erst alle Eingaben prüfen, dann das geladene Objekt ändern.
        // Sonst könnte bei ungültigem Dateinamen der Titel schon geändert sein.
        string title = RequiredText(command.Title, 200, "Title");
        string fileName = RequiredText(command.FileName, 255, "File name");
        Document document = await GetByIdAsync(id, ct);
        document.Title = title;
        document.FileName = fileName;
        await repository.UpdateAsync(document, ct);
        logger.LogInformation("Business: Updated document {DocumentId}.", id);
        return document;
    }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Document document = await GetByIdAsync(id, ct);
        await repository.DeleteAsync(document, ct);
        logger.LogInformation("Business: Deleted document {DocumentId}.", id);
    }
    public async Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default)
    {
        // Ohne Dokument gibt es 404. Eine leere Notizliste wäre sonst nicht
        // von einem vorhandenen Dokument ohne Notizen unterscheidbar.
        await GetByIdAsync(documentId, ct);
        IReadOnlyList<DocumentNote> notes = await repository.GetNotesAsync(documentId, ct);
        logger.LogInformation("Business: Loaded {NoteCount} notes for document {DocumentId}.", notes.Count, documentId);
        return notes;
    }
    public async Task<DocumentNote> AddNoteAsync(Guid documentId, CreateNoteCommand command, CancellationToken ct = default)
    {
        if (command == null)
        {
            throw new BusinessValidationException("Note input is required.");
        }
        string text = RequiredText(command.Text, 2000, "Note text");
        await GetByIdAsync(documentId, ct);
        DocumentNote note = new DocumentNote();
        note.Id = Guid.NewGuid();
        note.DocumentId = documentId;
        note.Text = text;
        note.CreatedAt = clock.GetUtcNow();
        await repository.AddNoteAsync(note, ct);
        logger.LogInformation("Business: Added note {NoteId} to document {DocumentId}.", note.Id, documentId);
        return note;
    }
    // Auch ohne API müssen die Regeln gelten, etwa bei direktem Aufruf in Tests.
    // Trim entfernt äußere Leerzeichen; nur Leerzeichen sind kein gültiger Text.
    private static string RequiredText(string? value, int maxLength, string field)
    {
        if (value == null)
        {
            throw new BusinessValidationException($"{field} must contain between 1 and {maxLength} characters.");
        }
        string trimmed = value.Trim();
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
        {
            throw new BusinessValidationException($"{field} must contain between 1 and {maxLength} characters.");
        }
        return trimmed;
    }
}
