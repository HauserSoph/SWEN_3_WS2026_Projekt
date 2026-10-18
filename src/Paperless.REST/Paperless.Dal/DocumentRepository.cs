using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Paperless.Domain;

namespace Paperless.Dal;

/// Liest und speichert Dokumente und Notizen in PostgreSQL.
public sealed class DocumentRepository : IDocumentRepository
{
    private readonly PaperlessDbContext db;

    public DocumentRepository(PaperlessDbContext db)
    {
        this.db = db;
        try
        {
            // Tabellen anlegen, falls sie noch fehlen. wird erst beim ersten tatsächlichen dokument anfrage/hochladen ausgeführt
            var creator = db.Database.GetService<IRelationalDatabaseCreator>()!;
            if (!creator.HasTables())
            {
                Console.WriteLine("DAL: Creating missing database tables.");
                creator.CreateTables();
                Console.WriteLine("DAL: Database tables created.");
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL database initialization failed: {exception}");
            throw;
        }
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            IReadOnlyList<Document> documents = await db.Documents.ToListAsync(ct);
            Console.WriteLine($"DAL: Read {documents.Count} documents.");
            return documents;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL reading documents failed: {exception}");
            throw;
        }
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            //gib mir das dokument mit der übergebenen Id, oder null, falls es nicht existiert
            Document? document = await db.Documents.FirstOrDefaultAsync(document => document.Id == id, ct);
            Console.WriteLine($"DAL: Looked up document {id}; found: {document != null}.");
            return document;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL reading document failed: {exception}");
            throw;
        }
    }

    public async Task AddAsync(Document document, CancellationToken ct = default)
    {
        try
        {
            //add erstellt nur das objekt, speicert es aber noch nicht
            db.Documents.Add(document);
            //hier wird gespichert
            await db.SaveChangesAsync(ct);
            Console.WriteLine($"DAL: Saved document {document.Id}.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL saving document failed: {exception}");
            throw;
        }
    }

    public async Task UpdateAsync(Document document, CancellationToken ct = default)
    {
        try
        {
            //geleiche wie oben
            db.Documents.Update(document);
            await db.SaveChangesAsync(ct);
            Console.WriteLine($"DAL: Updated document {document.Id}.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL updating document failed: {exception}");
            throw;
        }
    }

    public async Task DeleteAsync(Document document, CancellationToken ct = default)
    {
        try
        {
            //gleiches wie oben
            db.Documents.Remove(document);
            await db.SaveChangesAsync(ct);
            Console.WriteLine($"DAL: Deleted document {document.Id}.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL deleting document failed: {exception}");
            throw;
        }
    }

    public async Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default)
    {
        try
        {
            //gib mir die note bei der die Note_id (FK) zur übergebenen DocumentId passt
            IReadOnlyList<DocumentNote> notes = await db.Notes.Where(note => note.DocumentId == documentId).ToListAsync(ct);
            Console.WriteLine($"DAL: Read {notes.Count} notes for document {documentId}.");
            return notes;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL reading notes failed: {exception}");
            throw;
        }
    }

    public async Task AddNoteAsync(DocumentNote note, CancellationToken ct = default)
    {
        try
        {
            db.Notes.Add(note);
            await db.SaveChangesAsync(ct);
            Console.WriteLine($"DAL: Saved note {note.Id} for document {note.DocumentId}.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Console.WriteLine($"Error: DAL saving note failed: {exception}");
            throw;
        }
    }
}
