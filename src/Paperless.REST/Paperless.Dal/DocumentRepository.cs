using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Paperless.Domain;

namespace Paperless.Dal;
public sealed class DocumentRepository : IDocumentRepository
{
    // Der eingebaute Logger wird durch Dependency Injection übergeben.
    private readonly ILogger<DocumentRepository> logger;

    private readonly PaperlessDbContext db;

    public DocumentRepository(PaperlessDbContext db, ILogger<DocumentRepository> logger)
    {
        this.logger = logger;
        this.db = db;
        try
        {
            // Beim Erzeugen des Repositorys prüfen wir, ob überhaupt Tabellen vorhanden sind.
            // CreateTables legt das Modell nur an, wenn keine Tabellen existieren.
            // Bestehende Tabellen werden damit nicht aktualisiert; dafür wären Migrationen nötig.
            var creator = db.Database.GetService<IRelationalDatabaseCreator>()!;
            if (!creator.HasTables())
            {
                logger.LogInformation("DAL: Creating missing database tables.");
                creator.CreateTables();
                logger.LogInformation("DAL: Database tables created.");
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL database initialization failed");
            throw;
        }
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            IReadOnlyList<Document> documents = await db.Documents.ToListAsync(ct);
            logger.LogInformation("DAL: Read {DocumentCount} documents.", documents.Count);
            return documents;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL reading documents failed");
            throw;
        }
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            Document? document = await db.Documents.FirstOrDefaultAsync(document => document.Id == id, ct);
            logger.LogInformation("DAL: Looked up document {DocumentId}; found: {Found}.", id, document != null);
            return document;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL reading document failed");
            throw;
        }
    }

    public async Task AddAsync(Document document, CancellationToken ct = default)
    {
        try
        {
            // Add merkt das Objekt im DbContext vor. Erst SaveChangesAsync schreibt in die Datenbank.
            db.Documents.Add(document);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("DAL: Saved document {DocumentId}.", document.Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL saving document failed");
            throw;
        }
    }

    public async Task UpdateAsync(Document document, CancellationToken ct = default)
    {
        try
        {
            db.Documents.Update(document);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("DAL: Updated document {DocumentId}.", document.Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL updating document failed");
            throw;
        }
    }

    public async Task DeleteAsync(Document document, CancellationToken ct = default)
    {
        try
        {
            db.Documents.Remove(document);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("DAL: Deleted document {DocumentId}.", document.Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL deleting document failed");
            throw;
        }
    }

    public async Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default)
    {
        try
        {
            // DocumentId ist der Fremdschlüssel zum Dokument. EF übersetzt den Filter in SQL;
            // ToListAsync führt die Abfrage aus.
            IReadOnlyList<DocumentNote> notes = await db.Notes.Where(note => note.DocumentId == documentId).ToListAsync(ct);
            logger.LogInformation("DAL: Read {NoteCount} notes for document {DocumentId}.", notes.Count, documentId);
            return notes;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL reading notes failed");
            throw;
        }
    }

    public async Task AddNoteAsync(DocumentNote note, CancellationToken ct = default)
    {
        try
        {
            db.Notes.Add(note);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("DAL: Saved note {NoteId} for document {DocumentId}.", note.Id, note.DocumentId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Error: DAL saving note failed");
            throw;
        }
    }
}
