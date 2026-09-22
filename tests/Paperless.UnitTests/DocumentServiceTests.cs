using Moq;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.UnitTests;

public class DocumentServiceTests
{
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
