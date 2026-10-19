using AutoMapper;
using Paperless.Api.Contracts;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.Api.Mapping;
public class MappingConfiguration : Profile
{
    public MappingConfiguration()
    {
        // DTOs gehören zur API, Commands zur Business-Schicht. So braucht der Service kein HTTP.
        // Gleich benannte Felder ordnet AutoMapper automatisch zu.
        CreateMap<CreateDocumentDto, CreateDocumentCommand>();
        CreateMap<UpdateDocumentDto, UpdateDocumentCommand>();
        CreateMap<CreateNoteDto, CreateNoteCommand>();
        CreateMap<Document, DocumentDto>();
        CreateMap<DocumentNote, DocumentNoteDto>();
    }
}
