using System.ComponentModel.DataAnnotations;

namespace Paperless.Domain;

// Defines the shared document metadata model.
public class Document
{
    public Guid Id { get; set; }
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
