using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Paperless.Domain;

namespace Paperless.Dal;

/// Verbindung zu PostgreSQL. Zwei Tabellen: Dokumente und Notizen.
public sealed class PaperlessDbContext(DbContextOptions<PaperlessDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentNote> Notes => Set<DocumentNote>();

    // Normal SaveChanges calls also reach this overload. Check the data first.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    // Do the same checks when the application saves asynchronously.
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateChanges()
    {
        // ChangeTracker knows which documents were added or changed.
        foreach (EntityEntry<Document> entry in ChangeTracker.Entries<Document>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                Document document = entry.Entity;

                // Check the Required and StringLength attributes on the document.
                ValidationContext validationContext = new ValidationContext(document);
                Validator.ValidateObject(document, validationContext, validateAllProperties: true);

                if (document.Id == Guid.Empty)
                {
                    throw new ValidationException("Document ID must not be empty.");
                }
            }
        }

        // Check notes separately, using the same simple steps.
        foreach (EntityEntry<DocumentNote> entry in ChangeTracker.Entries<DocumentNote>())
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                DocumentNote note = entry.Entity;

                ValidationContext validationContext = new ValidationContext(note);
                Validator.ValidateObject(note, validationContext, validateAllProperties: true);

                if (note.Id == Guid.Empty)
                {
                    throw new ValidationException("Note ID must not be empty.");
                }

                if (note.DocumentId == Guid.Empty)
                {
                    throw new ValidationException("Document ID must not be empty.");
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Zeigt wie die Beziehung zwischen Dokumenten und Notizen ist
         modelBuilder.Entity<DocumentNote>()
            .HasOne<Document>()
            .WithMany()
            .HasForeignKey(note => note.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
