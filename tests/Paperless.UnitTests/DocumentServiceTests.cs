using Moq;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.UnitTests;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> repository = new(MockBehavior.Strict);
    private readonly DateTimeOffset now = new(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);
    private DocumentService Service => new(repository.Object, new FixedClock(now));

    [Fact]
    public async Task Create_TrimsInputAndGeneratesIdentityAndUtcTimestamp()
    {
        repository.Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await Service.CreateAsync(new(" Invoice ", " invoice.pdf "));
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Invoice", result.Title);
        Assert.Equal("invoice.pdf", result.FileName);
        Assert.Equal(now, result.CreatedAt);
        repository.Verify(r => r.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "file.pdf")]
    [InlineData("   ", "file.pdf")]
    [InlineData(null, "file.pdf")]
    [InlineData("Title", "")]
    [InlineData("Title", null)]
    public async Task Create_InvalidInputDoesNotSave(string? title, string? fileName)
    {
        await Assert.ThrowsAsync<BusinessValidationException>(() => Service.CreateAsync(new(title!, fileName!)));
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_RejectsOverlongValues()
    {
        await Assert.ThrowsAsync<BusinessValidationException>(() => Service.CreateAsync(new(new string('a', 201), "a.pdf")));
        await Assert.ThrowsAsync<BusinessValidationException>(() => Service.CreateAsync(new("Title", new string('a', 256))));
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_PreservesIdentityAndCreationTime()
    {
        var existing = new Document { Id = Guid.NewGuid(), Title = "Old", FileName = "old.pdf", CreatedAt = now.AddDays(-1) };
        repository.Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        repository.Setup(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await Service.UpdateAsync(existing.Id, new(" New ", " new.pdf "));
        Assert.Same(existing, result);
        Assert.Equal(now.AddDays(-1), result.CreatedAt);
        Assert.Equal("New", result.Title);
        Assert.Equal("new.pdf", result.FileName);
        repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_InvalidInputDoesNotReadOrWrite()
    {
        await Assert.ThrowsAsync<BusinessValidationException>(() => Service.UpdateAsync(Guid.NewGuid(), new("Title", "")));
        repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_UsesRepository()
    {
        var document = new Document { Id = Guid.NewGuid() };
        repository.Setup(r => r.GetByIdAsync(document.Id, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        repository.Setup(r => r.DeleteAsync(document, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        await Service.DeleteAsync(document.Id);
        repository.Verify(r => r.DeleteAsync(document, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FixedClock(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
