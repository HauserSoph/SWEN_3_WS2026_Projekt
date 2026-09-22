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
        // Tabellen anlegen, falls sie noch fehlen. wird erst beim ersten tatsächlichen dokument anfrage/hochladen ausgeführt
        var creator = db.Database.GetService<IRelationalDatabaseCreator>()!;
        if (!creator.HasTables())
            creator.CreateTables();
    }

    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default) =>
        await db.Documents.ToListAsync(ct);

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        //gib mir das dokument mit der übergebenen Id, oder null, falls es nicht existiert
        await db.Documents.FirstOrDefaultAsync(document => document.Id == id, ct);

    public async Task AddAsync(Document document, CancellationToken ct = default)
    {
        //add erstellt nur das objekt, speicert es aber noch nicht
        db.Documents.Add(document);
        //hier wird gespichert
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Document document, CancellationToken ct = default)
    {
        //geleiche wie oben
        db.Documents.Update(document);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Document document, CancellationToken ct = default)
    {
        //gleiches wie oben
        db.Documents.Remove(document);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default) =>
        //gib mir die note bei der die Note_id (FK) zur übergebenen DocumentId passt
        await db.Notes.Where(note => note.DocumentId == documentId).ToListAsync(ct);

    public async Task AddNoteAsync(DocumentNote note, CancellationToken ct = default)
    {
        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);
    }
}
