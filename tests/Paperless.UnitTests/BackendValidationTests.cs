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
    public async Task Create_InvalidFields_DoNotSave(string? title, string? fileName)
    {
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.CreateAsync(new CreateDocumentCommand(title!, fileName!)));

        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(201, 10)]
    [InlineData(10, 256)]
    public async Task Create_TooLongFields_DoNotSave(int titleLength, int fileNameLength)
    {
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(
            new CreateDocumentCommand(new string('t', titleLength), new string('f', fileNameLength))));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task NullCommands_AreRejectedBeforeRepositoryCalls()
    {
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.CreateAsync(null!));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.UpdateAsync(Guid.NewGuid(), null!));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.AddNoteAsync(Guid.NewGuid(), null!));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task EmptyDocumentIds_AreRejectedBeforeRepositoryCalls()
    {
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() => service.GetByIdAsync(Guid.Empty));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.DeleteAsync(Guid.Empty));
        await Assert.ThrowsAsync<BusinessValidationException>(() => service.GetNotesAsync(Guid.Empty));
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.UpdateAsync(Guid.Empty, new UpdateDocumentCommand("Title", "file.pdf")));
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.AddNoteAsync(Guid.Empty, new CreateNoteCommand("Note")));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidUpdate_DoesNotChangeTheExistingDocument()
    {
        Document document = new Document() { Id = Guid.NewGuid(), Title = "Original", FileName = "original.pdf" };
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        repository.Setup(r => r.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.UpdateAsync(document.Id, new UpdateDocumentCommand("New title", " ")));

        Assert.Equal("Original", document.Title);
        Assert.Equal("original.pdf", document.FileName);
        repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task InvalidNote_DoesNotSave(string? text)
    {
        Mock<IDocumentRepository> repository = new Mock<IDocumentRepository>();
        DocumentService service = new DocumentService(repository.Object, TimeProvider.System);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.AddNoteAsync(Guid.NewGuid(), new CreateNoteCommand(text!)));
        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            service.AddNoteAsync(Guid.NewGuid(), new CreateNoteCommand(new string('n', 2001))));

        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task MaximumLengths_AreAcceptedAndTrimmed()
    {
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
    public void CreateDocumentDto_RejectsBlankAndTooLongText()
    {
        CreateDocumentDto blank = new CreateDocumentDto(" ", "file.pdf");
        CreateDocumentDto tooLong = new CreateDocumentDto(new string('t', 201), "file.pdf");
        List<ValidationResult> errors = new List<ValidationResult>();

        bool blankIsValid = Validator.TryValidateObject(blank, new ValidationContext(blank), errors, true);
        errors.Clear();
        bool tooLongIsValid = Validator.TryValidateObject(tooLong, new ValidationContext(tooLong), errors, true);

        Assert.False(blankIsValid);
        Assert.False(tooLongIsValid);
    }

    [Fact]
    public void UpdateDocumentDto_RejectsBlankAndTooLongText()
    {
        UpdateDocumentDto blank = new UpdateDocumentDto("Title", " ");
        UpdateDocumentDto tooLong = new UpdateDocumentDto("Title", new string('f', 256));
        List<ValidationResult> errors = new List<ValidationResult>();

        bool blankIsValid = Validator.TryValidateObject(blank, new ValidationContext(blank), errors, true);
        errors.Clear();
        bool tooLongIsValid = Validator.TryValidateObject(tooLong, new ValidationContext(tooLong), errors, true);

        Assert.False(blankIsValid);
        Assert.False(tooLongIsValid);
    }

    [Fact]
    public void CreateNoteDto_RejectsBlankAndTooLongText()
    {
        CreateNoteDto blank = new CreateNoteDto(" ");
        CreateNoteDto tooLong = new CreateNoteDto(new string('n', 2001));
        List<ValidationResult> errors = new List<ValidationResult>();

        bool blankIsValid = Validator.TryValidateObject(blank, new ValidationContext(blank), errors, true);
        errors.Clear();
        bool tooLongIsValid = Validator.TryValidateObject(tooLong, new ValidationContext(tooLong), errors, true);

        Assert.False(blankIsValid);
        Assert.False(tooLongIsValid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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
    public async Task Dal_RejectsAnInvalidModifiedDocumentBeforeSaving()
    {
        using PaperlessDbContext db = CreateContext();
        Document document = new Document() { Id = Guid.NewGuid(), Title = "Title", FileName = "file.pdf" };
        db.Attach(document);
        document.FileName = new string('f', 256);

        await Assert.ThrowsAsync<ValidationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Dal_RejectsNotesWithoutAParentId()
    {
        using PaperlessDbContext db = CreateContext();
        db.Notes.Add(new DocumentNote { Id = Guid.NewGuid(), DocumentId = Guid.Empty, Text = "Note" });

        await Assert.ThrowsAsync<ValidationException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public void DatabaseModel_UsesRequiredFieldsAndMatchingLengthLimits()
    {
        using PaperlessDbContext db = CreateContext();
        IEntityType document = db.Model.FindEntityType(typeof(Document))!;
        IEntityType note = db.Model.FindEntityType(typeof(DocumentNote))!;

        Assert.Equal(200, document.FindProperty(nameof(Document.Title))!.GetMaxLength());
        Assert.Equal(255, document.FindProperty(nameof(Document.FileName))!.GetMaxLength());
        Assert.Equal(2000, note.FindProperty(nameof(DocumentNote.Text))!.GetMaxLength());
        Assert.False(document.FindProperty(nameof(Document.Title))!.IsNullable);
        Assert.False(document.FindProperty(nameof(Document.FileName))!.IsNullable);
        Assert.False(note.FindProperty(nameof(DocumentNote.Text))!.IsNullable);
    }

    private static PaperlessDbContext CreateContext()
    {
        DbContextOptions<PaperlessDbContext> options = new DbContextOptionsBuilder<PaperlessDbContext>()
            .UseNpgsql("Host=127.0.0.1;Database=validation;Username=test;Password=test")
            .Options;
        return new PaperlessDbContext(options);
    }
}
