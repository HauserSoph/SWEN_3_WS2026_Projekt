using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Paperless.Api.Contracts;
using Paperless.Business;
using Paperless.Domain;

namespace Paperless.Api.Controllers;
// ApiController prüft die DTO-Attribute vor dem Methodenaufruf und liefert bei Fehlern 400.
// Der Controller kümmert sich um HTTP und Mapping; die Regeln stehen im Service.
[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    // Der eingebaute Logger wird durch Dependency Injection übergeben.
    private readonly ILogger<DocumentsController> logger;

    private readonly IDocumentService service;
    private readonly IMapper mapper;
    public DocumentsController(IDocumentService service, IMapper mapper, ILogger<DocumentsController> logger)
    {
        this.logger = logger;
        this.service = service;
        this.mapper = mapper;
    }
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetAll(CancellationToken ct)
    {
        logger.LogInformation("API: GET /api/documents");
        IReadOnlyList<Document> documents = await service.GetAllAsync(ct);
        List<DocumentDto> response = new List<DocumentDto>();
        foreach (Document document in documents)
        {
            DocumentDto dto = mapper.Map<DocumentDto>(document);
            response.Add(dto);
        }
        return Ok(response);
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken ct)
    {
        logger.LogInformation("API: GET /api/documents/{DocumentId}", id);
        Document document = await service.GetByIdAsync(id, ct);
        DocumentDto response = mapper.Map<DocumentDto>(document);
        return Ok(response);
    }
    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Create(CreateDocumentDto dto, CancellationToken ct)
    {
        logger.LogInformation("API: POST /api/documents");
        CreateDocumentCommand command = mapper.Map<CreateDocumentCommand>(dto);
        Document document = await service.CreateAsync(command, ct);
        DocumentDto response = mapper.Map<DocumentDto>(document);
        RouteValueDictionary routeValues = new RouteValueDictionary();
        routeValues.Add("id", document.Id);
        // 201 bedeutet erstellt. Der Location-Header verweist auf die URL des neuen Dokuments.
        return CreatedAtAction(nameof(GetById), routeValues, response);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> Update(Guid id, UpdateDocumentDto dto, CancellationToken ct)
    {
        logger.LogInformation("API: PUT /api/documents/{DocumentId}", id);
        UpdateDocumentCommand command = mapper.Map<UpdateDocumentCommand>(dto);
        Document document = await service.UpdateAsync(id, command, ct);
        DocumentDto response = mapper.Map<DocumentDto>(document);
        return Ok(response);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        logger.LogInformation("API: DELETE /api/documents/{DocumentId}", id);
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
    [HttpGet("{id:guid}/notes")]
    public async Task<ActionResult<IReadOnlyList<DocumentNoteDto>>> GetNotes(Guid id, CancellationToken ct)
    {
        logger.LogInformation("API: GET /api/documents/{DocumentId}/notes", id);
        IReadOnlyList<DocumentNote> notes = await service.GetNotesAsync(id, ct);
        List<DocumentNoteDto> response = new List<DocumentNoteDto>();
        foreach (DocumentNote note in notes)
        {
            DocumentNoteDto dto = mapper.Map<DocumentNoteDto>(note);
            response.Add(dto);
        }
        return Ok(response);
    }
    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<DocumentNoteDto>> AddNote(Guid id, CreateNoteDto dto, CancellationToken ct)
    {
        logger.LogInformation("API: POST /api/documents/{DocumentId}/notes", id);
        CreateNoteCommand command = mapper.Map<CreateNoteCommand>(dto);
        DocumentNote note = await service.AddNoteAsync(id, command, ct);
        DocumentNoteDto response = mapper.Map<DocumentNoteDto>(note);
        RouteValueDictionary routeValues = new RouteValueDictionary();
        routeValues.Add("id", id);
        return CreatedAtAction(nameof(GetNotes), routeValues, response);
    }
}
