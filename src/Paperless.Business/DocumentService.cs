using Paperless.Domain;

namespace Paperless.Business;

// Receives the repository and clock through DI and implements the business contract.
public class DocumentService : IDocumentService
{
    // Keeps the repository supplied by dependency injection.
    private readonly IDocumentRepository repository;
    // Keeps the clock used to set creation times.
    private readonly TimeProvider clock;

    // Receives and stores the dependencies needed by this service.
    public DocumentService(IDocumentRepository repository, TimeProvider clock)
    {
        this.repository = repository;
        this.clock = clock;
    }
    // Returns all documents from the repository.
    public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default)
    {
        // Waits for the repository to load the documents.
        IReadOnlyList<Document> documents = await repository.GetAllAsync(ct);
        // Returns the loaded list.
        return documents;
    }

    // Loads a document or reports that it does not exist.
    public async Task<Document> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Looks up the document
        Document? document = await repository.GetByIdAsync(id, ct);
        // Checks whether the repository found a document.
        if (document == null)
        {
            throw new DocumentNotFoundException(id);
        }
        return document;
    }

    // Creates a document from the supplied business input.
    public async Task<Document> CreateAsync(CreateDocumentCommand command, CancellationToken ct = default)
    {
        // Creates an empty document object.
        Document document = new Document();
        // Gives the new document a unique ID.
        document.Id = Guid.NewGuid();
        // Checks and stores the title.
        document.Title = RequiredText(command.Title, 200, "Title");
        // Checks and stores the file name.
        document.FileName = RequiredText(command.FileName, 255, "File name");
        // Stores the current UTC time.
        document.CreatedAt = clock.GetUtcNow();
        // Waits for the repository to save the new document.
        await repository.AddAsync(document, ct);
        // Returns the document to the caller.
        return document;
    }

    // Updates the editable metadata of an existing document.
    public async Task<Document> UpdateAsync(Guid id, UpdateDocumentCommand command, CancellationToken ct = default)
    {
        // Validates the new title before changing the existing document.
        string title = RequiredText(command.Title, 200, "Title");
        // Validates the new file name before changing the existing document.
        string fileName = RequiredText(command.FileName, 255, "File name");
        // Loads the document or throws an error if it is missing.
        Document document = await GetByIdAsync(id, ct);
        // Replaces the title with the validated value.
        document.Title = title;
        // Replaces the file name while keeping the ID and creation time.
        document.FileName = fileName;
        // Waits for the repository to save the changes.
        await repository.UpdateAsync(document, ct);
        // Returns the document to the caller.
        return document;
    }

    // Defines the business operation for deleting a document.
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        Document document = await GetByIdAsync(id, ct);
        // Asks the repository to delete the document and its notes.
        await repository.DeleteAsync(document, ct);
    }

    // Defines the business operation for listing a document's notes.
    public async Task<IReadOnlyList<DocumentNote>> GetNotesAsync(Guid documentId, CancellationToken ct = default)
    {
        // Checks that the parent document exists before working with its notes.
        await GetByIdAsync(documentId, ct);
        // Loads and returns the notes belonging to that document.
        return await repository.GetNotesAsync(documentId, ct);
    }

    // Defines the business operation for adding a note to a document.
    public async Task<DocumentNote> AddNoteAsync(Guid documentId, CreateNoteCommand command, CancellationToken ct = default)
    {
        // Trims and validates the note with a 2000-character limit.
        string text = RequiredText(command.Text, 2000, "Note text");
        // Checks that the parent document exists before working with its notes.
        await GetByIdAsync(documentId, ct);
        // Creates an empty note object.
        DocumentNote note = new DocumentNote();
        // Gives the note a unique ID.
        note.Id = Guid.NewGuid();
        // Connects the note to its document.
        note.DocumentId = documentId;
        // Stores the checked note text.
        note.Text = text;
        // Stores the current UTC time.
        note.CreatedAt = clock.GetUtcNow();
        // Waits for the repository to save the note.
        await repository.AddNoteAsync(note, ct);
        // Returns the new note to the caller.
        return note;
    }

    // Defines a helper that checks required text; the input may be null.
    private static string RequiredText(string? value, int maxLength, string field)
    {
        // Rejects missing input before trying to trim it.
        if (value == null)
        {
            // Explains which field needs a value.
            throw new BusinessValidationException($"{field} must contain between 1 and {maxLength} characters.");
        }
        // Removes spaces from the start and end of the input.
        string trimmed = value.Trim();
        // Checks whether the cleaned text is empty or too long.
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
        {
            throw new BusinessValidationException($"{field} must contain between 1 and {maxLength} characters.");
        }
        // Returns the cleaned and checked text.
        return trimmed;
    }
}
