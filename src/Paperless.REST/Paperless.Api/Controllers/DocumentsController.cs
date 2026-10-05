using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Paperless.Api.Contracts;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.Api.Controllers;

// Enables automatic request validation.
[ApiController]
// Sets the shared URL for document requests.
[Route("api/documents")]
// Receives requests and calls the business service.
public class DocumentsController : ControllerBase
{
    // Keeps the business service for this controller.
    private readonly IDocumentService service;
    // Keeps the mapper for converting requests and responses.
    private readonly IMapper mapper;

    // Receives the dependencies from ASP.NET Core.
    public DocumentsController(IDocumentService service, IMapper mapper)
    {
        // Stores the supplied business service.
        this.service = service;
        // Stores the supplied mapper.
        this.mapper = mapper;
    }

    // Handles GET /api/documents.
    [HttpGet]
    // Returns all documents as response DTOs.
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetAll(CancellationToken ct)
    {
        // Loads the documents through the business service.
        IReadOnlyList<Document> documents = await service.GetAllAsync(ct);
        // Creates the list that will be sent to the caller.
        List<DocumentDto> response = new List<DocumentDto>();
        // Goes through the documents one at a time.
        foreach (Document document in documents)
        {
            // Converts this document into a response DTO.
            DocumentDto dto = mapper.Map<DocumentDto>(document);
            // Adds the DTO to the response list.
            response.Add(dto);
        }
        // Returns HTTP 200 with the list.
        return Ok(response);
    }

    // Handles GET requests with a document ID.
    [HttpGet("{id:guid}")]
    // Returns one document or lets the error handler report a missing ID.
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken ct)
    {
        // Loads the requested document.
        Document document = await service.GetByIdAsync(id, ct);
        // Converts the document into a response DTO.
        DocumentDto response = mapper.Map<DocumentDto>(document);
        // Returns HTTP 200 with the document.
        return Ok(response);
    }

    // Handles POST /api/documents.
    [HttpPost]
    // Creates a document from the request data.
    public async Task<ActionResult<DocumentDto>> Create(CreateDocumentDto dto, CancellationToken ct)
    {
        // Converts the request into business input.
        CreateDocumentCommand command = mapper.Map<CreateDocumentCommand>(dto);
        // Asks the business service to create the document.
        Document document = await service.CreateAsync(command, ct);
        // Converts the result into a response DTO.
        DocumentDto response = mapper.Map<DocumentDto>(document);
        // Stores the ID used to build the URL of the new document.
        RouteValueDictionary routeValues = new RouteValueDictionary();
        // Adds the document ID to the URL values.
        routeValues.Add("id", document.Id);
        // Returns HTTP 201 with the response and a link to GetById.
        return CreatedAtAction(nameof(GetById), routeValues, response);
    }

    // Handles PUT requests with a document ID.
    [HttpPut("{id:guid}")]
    // Updates the title and file name of a document.
    public async Task<ActionResult<DocumentDto>> Update(Guid id, UpdateDocumentDto dto, CancellationToken ct)
    {
        // Converts the request into business input.
        UpdateDocumentCommand command = mapper.Map<UpdateDocumentCommand>(dto);
        // Asks the service to update the document.
        Document document = await service.UpdateAsync(id, command, ct);
        // Converts the updated document into a response DTO.
        DocumentDto response = mapper.Map<DocumentDto>(document);
        // Returns HTTP 200 with the updated document.
        return Ok(response);
    }

    // Handles DELETE requests with a document ID.
    [HttpDelete("{id:guid}")]
    // Deletes the document through the business service.
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // Waits until the service finishes deleting the document.
        await service.DeleteAsync(id, ct);
        // Returns HTTP 204 without a response body.
        return NoContent();
    }

    // Handles GET requests for a document's notes.
    [HttpGet("{id:guid}/notes")]
    // Returns the notes belonging to a document.
    public async Task<ActionResult<IReadOnlyList<DocumentNoteDto>>> GetNotes(Guid id, CancellationToken ct)
    {
        // Loads the notes through the business service.
        IReadOnlyList<DocumentNote> notes = await service.GetNotesAsync(id, ct);
        // Creates the response list.
        List<DocumentNoteDto> response = new List<DocumentNoteDto>();
        // Goes through the notes one at a time.
        foreach (DocumentNote note in notes)
        {
            // Converts this note into a response DTO.
            DocumentNoteDto dto = mapper.Map<DocumentNoteDto>(note);
            // Adds the DTO to the response list.
            response.Add(dto);
        }
        // Returns HTTP 200 with the notes.
        return Ok(response);
    }

    // Handles POST requests for a document's notes.
    [HttpPost("{id:guid}/notes")]
    // Adds a note to the requested document.
    public async Task<ActionResult<DocumentNoteDto>> AddNote(Guid id, CreateNoteDto dto, CancellationToken ct)
    {
        // Converts the request into business input.
        CreateNoteCommand command = mapper.Map<CreateNoteCommand>(dto);
        // Asks the service to create the note.
        DocumentNote note = await service.AddNoteAsync(id, command, ct);
        // Converts the saved note into a response DTO.
        DocumentNoteDto response = mapper.Map<DocumentNoteDto>(note);
        // Creates the values used for the note-list URL.
        RouteValueDictionary routeValues = new RouteValueDictionary();
        // Adds the parent document ID to the URL values.
        routeValues.Add("id", id);
        // Returns HTTP 201 with the note and a link to GetNotes.
        return CreatedAtAction(nameof(GetNotes), routeValues, response);
    }
}
