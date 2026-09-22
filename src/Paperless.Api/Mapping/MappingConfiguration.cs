using Mapster;
using Paperless.Api.Contracts;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.Api.Mapping;

public static class MappingConfiguration
{
    /// Builds and compiles the mappings between API DTOs, business commands and domain models.
    public static TypeAdapterConfig Create()
    {
        var config = new TypeAdapterConfig { RequireExplicitMapping = true };
        config.NewConfig<CreateDocumentDto, CreateDocumentCommand>()
            .MapWith(dto => new CreateDocumentCommand(dto.Title, dto.FileName));
        config.NewConfig<UpdateDocumentDto, UpdateDocumentCommand>()
            .MapWith(dto => new UpdateDocumentCommand(dto.Title, dto.FileName));
        config.NewConfig<CreateNoteDto, CreateNoteCommand>()
            .MapWith(dto => new CreateNoteCommand(dto.Text));
        config.NewConfig<Document, DocumentDto>()
            .MapWith(d => new DocumentDto(d.Id, d.Title, d.FileName, d.CreatedAt));
        config.NewConfig<DocumentNote, DocumentNoteDto>()
            .MapWith(n => new DocumentNoteDto(n.Id, n.DocumentId, n.Text, n.CreatedAt));
        config.Compile();
        return config;
    }
}
