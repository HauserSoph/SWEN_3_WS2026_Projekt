using AutoMapper;
using Paperless.Api.Contracts;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.Api.Mapping;

// Groups the AutoMapper rules in one profile.
public class MappingConfiguration : Profile
{
    // Registers the mappings when this profile is created.
    public MappingConfiguration()
    {
        // Maps create-request fields to the business command.
        CreateMap<CreateDocumentDto, CreateDocumentCommand>();
        // Maps update-request fields to the business command.
        CreateMap<UpdateDocumentDto, UpdateDocumentCommand>();
        // Maps note input to the business command.
        CreateMap<CreateNoteDto, CreateNoteCommand>();
        // Maps a document to the response DTO.
        CreateMap<Document, DocumentDto>();
        // Maps a note to the response DTO.
        CreateMap<DocumentNote, DocumentNoteDto>();
    }
}
