using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Paperless.Api.Contracts;
using Paperless.Api.Messaging;
using Paperless.Business;
using Paperless.Domain;
using Microsoft.Extensions.Options;

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
    private readonly IDocumentUploadPublisher uploadPublisher;
    private readonly RabbitMqOptions rabbitMqOptions;
    public DocumentsController(IDocumentService service, IMapper mapper, ILogger<DocumentsController> logger,
        IDocumentUploadPublisher uploadPublisher, IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        this.logger = logger;
        this.service = service;
        this.mapper = mapper;
        this.uploadPublisher = uploadPublisher;
        this.rabbitMqOptions = rabbitMqOptions.Value;
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
    [Consumes("application/json")]
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

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<ActionResult<DocumentDto>> Upload([FromForm] string title, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
        {
            throw new BusinessValidationException("Title must contain between 1 and 200 characters.");
        }
        if (file == null || file.Length == 0)
        {
            throw new BusinessValidationException("A non-empty file is required.");
        }
        if (file.FileName.Length > 255)
        {
            throw new BusinessValidationException("File name must contain between 1 and 255 characters.");
        }

        string fileName = Path.GetFileName(file.FileName);
        string incomingPath = Path.GetFullPath(rabbitMqOptions.IncomingPath);
        Directory.CreateDirectory(incomingPath);
        string stagedFilePath = Path.Combine(incomingPath, $"{Guid.NewGuid():N}.upload");
        try
        {
            await using (FileStream stream = System.IO.File.Create(stagedFilePath))
            {
                await file.CopyToAsync(stream, ct);
            }

            Document document = await service.CreateAsync(
                new CreateDocumentCommand(title, fileName), ct);
            DocumentUploadJob job = new DocumentUploadJob(
                1, document.Id, fileName, file.ContentType ?? "application/octet-stream", file.Length, stagedFilePath);
            await uploadPublisher.PublishAsync(job, ct);

            DocumentDto response = mapper.Map<DocumentDto>(document);
            RouteValueDictionary routeValues = new RouteValueDictionary { ["id"] = document.Id };
            return AcceptedAtAction(nameof(GetById), routeValues, response);
        }
        catch
        {
            if (System.IO.File.Exists(stagedFilePath))
            {
                System.IO.File.Delete(stagedFilePath);
            }
            throw;
        }
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
