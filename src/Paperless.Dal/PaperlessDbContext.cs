using Microsoft.EntityFrameworkCore;
using Paperless.Domain;

namespace Paperless.Dal;

/// Verbindung zu PostgreSQL. Zwei Tabellen: Dokumente und Notizen.
public sealed class PaperlessDbContext(DbContextOptions<PaperlessDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentNote> Notes => Set<DocumentNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // zeigt wie die Beziehung zwischen Dokumenten und Notizen ist
         modelBuilder.Entity<DocumentNote>()
            .HasOne<Document>()
            .WithMany()
            .HasForeignKey(note => note.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
