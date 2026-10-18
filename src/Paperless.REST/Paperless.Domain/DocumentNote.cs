using System.ComponentModel.DataAnnotations;

namespace Paperless.Domain;

// Defines the shared model for a note attached to a document.
public class DocumentNote
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    [Required, StringLength(2000)]
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
