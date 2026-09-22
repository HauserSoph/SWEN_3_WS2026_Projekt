namespace Paperless.Domain;

public sealed class DocumentNote
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
