using Moq;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.UnitTests;

public class DocumentServiceTests
{
    [Fact]
    // Check that creating a document fills its fields and requests a save.
    public async Task Create_SetsIdAndCreationTime_AndSavesDocument()
    {
        // Use a fixed time so the expected creation time is predictable.
        DateTimeOffset now = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
        Mock<TimeProvider> clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow()).Returns(now);
        // Replace the database with a mock, but use the real business service.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, clock.Object);
        // The command contains the input that would normally arrive from the API.
        CreateDocumentCommand command = new CreateDocumentCommand("Invoice", "invoice.pdf");

        // Create a document using the same method the API calls.
        Document result = await service.CreateAsync(command);

        // Check that the service generates an ID and stores the supplied metadata.
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Invoice", result.Title);
        Assert.Equal("invoice.pdf", result.FileName);
        // The creation time must match the time returned by our mock clock.
        Assert.Equal(now, result.CreatedAt);
        // Verify that this document was passed to the repository exactly once.
        // It.IsAny means the cancellation token's value does not matter for this test.
        repository.Verify(r => r.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    // Check that editing a document changes only its editable metadata.
    public async Task Update_ChangesMetadata_ButKeepsIdAndCreationTime()
    {
        // Prepare a document that already exists before the update.
        Guid documentId = Guid.NewGuid();
        DateTimeOffset createdAt = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
        Document document = new Document();
        document.Id = documentId;
        document.Title = "Original";
        document.FileName = "original.pdf";
        document.CreatedAt = createdAt;

        // The repository returns the existing document for the update.
        // Setup tells the mock what to return when the service calls GetByIdAsync.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        repository.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);
        UpdateDocumentCommand command = new UpdateDocumentCommand("Updated", "updated.pdf");

        // Change the document's title and file name through the service.
        Document result = await service.UpdateAsync(documentId, command);

        // Editable fields should contain the new values.
        Assert.Equal("Updated", result.Title);
        Assert.Equal("updated.pdf", result.FileName);
        // Updating metadata must not replace the document's identity or creation time.
        Assert.Equal(documentId, result.Id);
        Assert.Equal(createdAt, result.CreatedAt);
        // Save an update once; do not create another document.
        repository.Verify(r => r.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    // Check that the document found by its ID is passed on for deletion.
    public async Task Delete_PassesTheExistingDocumentToTheRepository()
    {
        // Prepare the document that the service should find and delete.
        Document document = new Document();
        document.Id = Guid.NewGuid();
        document.Title = "Invoice";
        document.FileName = "invoice.pdf";

        // Return this document when the service looks up its ID.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        repository.Setup(r => r.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        // Ask the real service to delete the document by its ID.
        await service.DeleteAsync(document.Id);

        // Check the deletion request. The mock does not actually delete database rows.
        repository.Verify(r => r.DeleteAsync(document, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    // Check that a new note belongs to the correct document and is passed on for saving.
    public async Task AddNote_SetsIdParentAndCreationTime_AndSavesNote()
    {
        // A note needs an existing document to belong to.
        Document document = new Document();
        document.Id = Guid.NewGuid();
        // Keep the clock predictable so the note's creation time can be checked.
        DateTimeOffset now = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
        Mock<TimeProvider> clock = new Mock<TimeProvider>();
        clock.Setup(c => c.GetUtcNow()).Returns(now);

        // Let the service find the parent document without using a database.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        repository.Setup(r => r.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        DocumentService service = new DocumentService(repository.Object, clock.Object);
        CreateNoteCommand command = new CreateNoteCommand("Check the invoice.");

        // Add the note through the real business service.
        DocumentNote result = await service.AddNoteAsync(document.Id, command);

        // Check the new ID, the link to the parent document, the text and the time.
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(document.Id, result.DocumentId);
        Assert.Equal("Check the invoice.", result.Text);
        Assert.Equal(now, result.CreatedAt);
        // Confirm that the service passed this note to the repository once.
        repository.Verify(r => r.AddNoteAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    // Tests that an empty title causes a business validation error.
    public async Task Create_EmptyTitle_IsRejected()
    {
        // Creates a mock repository so this test needs no database.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        // Creates the real service using the mock repository and system clock.
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        // Stores the invalid input for this test.
        CreateDocumentCommand command = new CreateDocumentCommand("", "rechnung.pdf");

        // Checks that the following asynchronous call throws the expected error.
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            // Calls the service with an empty title to trigger validation.
            service.CreateAsync(command));

        // Moq uses this lambda expression to check that no document was saved.
        repository.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
