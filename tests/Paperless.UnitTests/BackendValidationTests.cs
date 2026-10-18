using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using Paperless.Api.Contracts;
using Paperless.Business;
using Paperless.Dal;
using Paperless.Domain;

namespace Paperless.UnitTests;

public class BackendValidationTests
{
    [Theory]
    [InlineData(null, "file.pdf")]
    [InlineData("   ", "file.pdf")]
    [InlineData("Title", null)]
    [InlineData("Title", "   ")]
    // Try missing or whitespace-only fields. Each InlineData row runs this test again.
    public async Task Create_InvalidFields_DoNotSave(string? title, string? fileName)
    {
        // Use a fake repository so this test does not need PostgreSQL.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        // The ! lets us deliberately pass null in the test; it does not check the value.
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.CreateAsync(new CreateDocumentCommand(title!, fileName!)));

        // The service should reject this input before reading or writing any data.
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(201, 10)]
    [InlineData(10, 256)]
    // Check values just above the allowed title and file-name lengths.
    public async Task Create_TooLongFields_DoNotSave(int titleLength, int fileNameLength)
    {
        // Use a fake repository so this test does not need PostgreSQL.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(
            new CreateDocumentCommand(new string('t', titleLength), new string('f', fileNameLength))));

        // The service should reject this input before reading or writing any data.
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    // A missing command should produce a validation error instead of a null-reference error.
    public async Task NullCommands_AreRejectedBeforeRepositoryCalls()
    {
        // Use a fake repository so this test does not need PostgreSQL.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(null!));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateAsync(Guid.NewGuid(), null!));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.AddNoteAsync(Guid.NewGuid(), null!));

        // The service should reject this input before reading or writing any data.
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    // Guid.Empty is an ID made entirely of zeros. No repository call should use it.
    public async Task EmptyDocumentIds_AreRejectedBeforeRepositoryCalls()
    {
        // Use a fake repository so this test does not need PostgreSQL.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.GetByIdAsync(Guid.Empty));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.DeleteAsync(Guid.Empty));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.GetNotesAsync(Guid.Empty));
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.UpdateAsync(Guid.Empty, new UpdateDocumentCommand("Title", "file.pdf")));
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.AddNoteAsync(Guid.Empty, new CreateNoteCommand("Note")));

        // The service should reject this input before reading or writing any data.
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    // An invalid update must leave the original document unchanged.
    public async Task InvalidUpdate_DoesNotChangeTheExistingDocument()
    {
        Document document = new Document() { Id = Guid.NewGuid(), Title = "Original", FileName = "original.pdf" };
        // Use a fake repository so this test does not need PostgreSQL.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        repository.Setup(r => r.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.UpdateAsync(document.Id, new UpdateDocumentCommand("New title", " ")));

        Assert.Equal("Original", document.Title);
        Assert.Equal("original.pdf", document.FileName);
        // The service should reject this input before reading or writing any data.
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    // Reject missing, blank and oversized note text before saving anything.
    public async Task InvalidNote_DoesNotSave(string? text)
    {
        // Use a fake repository so this test does not need PostgreSQL.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.AddNoteAsync(Guid.NewGuid(), new CreateNoteCommand(text!)));
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.AddNoteAsync(Guid.NewGuid(), new CreateNoteCommand(new string('n', 2001))));

        // The service should reject this input before reading or writing any data.
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    // Values at the allowed limits should work after surrounding spaces are removed.
    public async Task MaximumLengths_AreAcceptedAndTrimmed()
    {
        // Use a fake repository so this test does not need PostgreSQL.
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);
        string title = new string('t', 200);
        string fileName = new string('f', 255);
        Document document = await service.CreateAsync(new CreateDocumentCommand(" " + title + " ", fileName));
        repository.Setup(r => r.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        Document updated = await service.UpdateAsync(document.Id, new UpdateDocumentCommand(title, fileName));
        DocumentNote note = await service.AddNoteAsync(document.Id, new CreateNoteCommand(" " + new string('n', 2000) + " "));

        Assert.Equal(title, updated.Title);
        Assert.Equal(fileName, updated.FileName);
        Assert.Equal(2000, note.Text.Length);
        repository.Verify(r => r.AddAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(r => r.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(r => r.AddNoteAsync(note, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    // Check the validation attributes on the API input for creating documents.
    public void CreateDocumentDto_RejectsBlankAndTooLongText()
    {
        CreateDocumentDto blank = new CreateDocumentDto(" ", "file.pdf");
        CreateDocumentDto tooLong = new CreateDocumentDto(new string('t', 201), "file.pdf");
        List<ValidationResult> errors = new List<ValidationResult>();

        // Run the Required and StringLength checks and collect any errors.
        bool blankIsValid = Validator.TryValidateObject(blank, new ValidationContext(blank), errors, true);
        // Clear the first result before checking the second object.
        errors.Clear();
        bool tooLongIsValid = Validator.TryValidateObject(tooLong, new ValidationContext(tooLong), errors, true);

        // Neither invalid object should pass validation.
        Assert.False(blankIsValid);
        Assert.False(tooLongIsValid);
    }

    [Fact]
    // Check the validation attributes on the API input for updating documents.
    public void UpdateDocumentDto_RejectsBlankAndTooLongText()
    {
        UpdateDocumentDto blank = new UpdateDocumentDto("Title", " ");
        UpdateDocumentDto tooLong = new UpdateDocumentDto("Title", new string('f', 256));
        List<ValidationResult> errors = new List<ValidationResult>();

        // Run the Required and StringLength checks and collect any errors.
        bool blankIsValid = Validator.TryValidateObject(blank, new ValidationContext(blank), errors, true);
        // Clear the first result before checking the second object.
        errors.Clear();
        bool tooLongIsValid = Validator.TryValidateObject(tooLong, new ValidationContext(tooLong), errors, true);

        // Neither invalid object should pass validation.
        Assert.False(blankIsValid);
        Assert.False(tooLongIsValid);
    }

    [Fact]
    // Check the validation attributes on the API input for adding notes.
    public void CreateNoteDto_RejectsBlankAndTooLongText()
    {
        CreateNoteDto blank = new CreateNoteDto(" ");
        CreateNoteDto tooLong = new CreateNoteDto(new string('n', 2001));
        List<ValidationResult> errors = new List<ValidationResult>();

        // Run the Required and StringLength checks and collect any errors.
        bool blankIsValid = Validator.TryValidateObject(blank, new ValidationContext(blank), errors, true);
        // Clear the first result before checking the second object.
        errors.Clear();
        bool tooLongIsValid = Validator.TryValidateObject(tooLong, new ValidationContext(tooLong), errors, true);

        // Neither invalid object should pass validation.
        Assert.False(blankIsValid);
        Assert.False(tooLongIsValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    // Test both save methods. Invalid data must be rejected before a database connection is needed.
    public async Task Dal_RejectsInvalidEntitiesBeforeConnectingToDatabase(bool asynchronous)
    {
        using PaperlessDbContext db = CreateContext();
        db.Documents.Add(new Document { Id = Guid.NewGuid(), Title = " ", FileName = "file.pdf" });

        if (asynchronous)
        {
            await Assert.ThrowsAsync<ValidationException>(() => db.SaveChangesAsync());
        }
        else
        {
            Assert.Throws<ValidationException>(() => db.SaveChanges());
        }
    }

    [Fact]
    // Validation must also check changes to existing documents, not only new documents.
    public async Task Dal_RejectsAnInvalidModifiedDocumentBeforeSaving()
    {
        using PaperlessDbContext db = CreateContext();
        Document document = new Document() { Id = Guid.NewGuid(), Title = "Title", FileName = "file.pdf" };
        // Track this object as an existing document, then change its file name.
        db.Attach(document);
        document.FileName = new string('f', 256);

        await Assert.ThrowsAsync<ValidationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    // A note must contain the ID of the document it belongs to.
    public async Task Dal_RejectsNotesWithoutAParentId()
    {
        using PaperlessDbContext db = CreateContext();
        db.Notes.Add(new DocumentNote { Id = Guid.NewGuid(), DocumentId = Guid.Empty, Text = "Note" });

        await Assert.ThrowsAsync<ValidationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    // Inspect the EF model settings; this does not query a real database.
    public void DatabaseModel_UsesRequiredFieldsAndMatchingLengthLimits()
    {
        using PaperlessDbContext db = CreateContext();
        // Read the metadata EF uses to describe the document and note tables.
        IEntityType document = db.Model.FindEntityType(typeof(Document))!;
        IEntityType note = db.Model.FindEntityType(typeof(DocumentNote))!;

        Assert.Equal(200, document.FindProperty(nameof(Document.Title))!.GetMaxLength());
        Assert.Equal(255, document.FindProperty(nameof(Document.FileName))!.GetMaxLength());
        Assert.Equal(2000, note.FindProperty(nameof(DocumentNote.Text))!.GetMaxLength());
        Assert.False(document.FindProperty(nameof(Document.Title))!.IsNullable);
        Assert.False(document.FindProperty(nameof(Document.FileName))!.IsNullable);
        Assert.False(note.FindProperty(nameof(DocumentNote.Text))!.IsNullable);
    }

    // Configure a context for the DAL checks. These tests never execute database queries.
    private static PaperlessDbContext CreateContext()
    {
        DbContextOptions<PaperlessDbContext> options = new DbContextOptionsBuilder<PaperlessDbContext>()
            .UseNpgsql("Host=127.0.0.1;Database=validation;Username=test;Password=test")
            .Options;
        return new PaperlessDbContext(options);
    }
}
