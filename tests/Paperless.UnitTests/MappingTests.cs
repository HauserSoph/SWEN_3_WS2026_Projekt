using MapsterMapper;
using Paperless.Api.Contracts;
using Paperless.Api.Mapping;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.UnitTests;

public class MappingTests
{
    private readonly Mapper mapper = new(MappingConfiguration.Create());

    [Fact]
    public void Requests_MapToBusinessCommands()
    {
        Assert.Equal(new CreateDocumentCommand("Title", "a.pdf"),
            mapper.Map<CreateDocumentCommand>(new CreateDocumentDto("Title", "a.pdf")));
        Assert.Equal(new UpdateDocumentCommand("New", "b.pdf"),
            mapper.Map<UpdateDocumentCommand>(new UpdateDocumentDto("New", "b.pdf")));
        Assert.Equal(new CreateNoteCommand("Review"),
            mapper.Map<CreateNoteCommand>(new CreateNoteDto("Review")));
    }

    [Fact]
    public void DocumentResponse_PreservesEveryPublicField()
    {
        var document = new Document
        {
            Id = Guid.NewGuid(), Title = "Invoice", FileName = "invoice.pdf", CreatedAt = DateTimeOffset.UtcNow
        };
        Assert.Equal(new DocumentDto(document.Id, document.Title, document.FileName, document.CreatedAt),
            mapper.Map<DocumentDto>(document));
    }

    [Fact]
    public void NoteResponse_PreservesParentAndContent()
    {
        var note = new DocumentNote
        {
            Id = Guid.NewGuid(), DocumentId = Guid.NewGuid(), Text = "Check", CreatedAt = DateTimeOffset.UtcNow
        };
        Assert.Equal(new DocumentNoteDto(note.Id, note.DocumentId, note.Text, note.CreatedAt),
            mapper.Map<DocumentNoteDto>(note));
    }
}
