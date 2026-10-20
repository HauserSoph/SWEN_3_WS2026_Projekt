using Paperless.Domain;

namespace Paperless.Business;
public class CreateDocumentCommand
{
    public string Title { get; init; }

    public string FileName { get; init; }
    public CreateDocumentCommand(string title, string fileName)
    {
        Title = title;
        FileName = fileName;
    }
}
public class UpdateDocumentCommand
{
    public string Title { get; init; }

    public string FileName { get; init; }

    public UpdateDocumentCommand(string title, string fileName)
    {
        Title = title;
        FileName = fileName;
    }
}
public class CreateNoteCommand
{
    public string Text { get; init; }

    public CreateNoteCommand(string text)
    {
        Text = text;
    }
}
public interface IDocumentService
{
    Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default);
    Task<Document> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Document> CreateAsync(CreateDocumentCommand command, CancellationToken ct = default);
    Task<Document> UpdateAsync(Guid id, UpdateDocumentCommand command, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default);
    Task<DocumentNote> AddNoteAsync(Guid documentId, CreateNoteCommand command, CancellationToken ct = default);
}
