using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Paperless.Api.Contracts;
using Paperless.Business;

namespace Paperless.Api.Controllers;

[ApiController]
[Route("api/documents")]
public sealed class DocumentsController(IDocumentService service, IMapper mapper) : ControllerBase
{
    /// Returns all documents as response DTOs with HTTP 200.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentDto>>> GetAll(CancellationToken ct) =>
        Ok((await service.GetAllAsync(ct)).Select(mapper.Map<DocumentDto>).ToList());

    /// Returns a single document as a response DTO.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(mapper.Map<DocumentDto>(await service.GetByIdAsync(id, ct)));

    /// Creates document metadata and returns HTTP 201 with its resource URL.
    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Create(CreateDocumentDto dto, CancellationToken ct)
    {
        var document = await service.CreateAsync(mapper.Map<CreateDocumentCommand>(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = document.Id }, mapper.Map<DocumentDto>(document));
    }

    /// Replaces editable document metadata and returns the updated response DTO.
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DocumentDto>> Update(Guid id, UpdateDocumentDto dto, CancellationToken ct) =>
        Ok(mapper.Map<DocumentDto>(await service.UpdateAsync(id, mapper.Map<UpdateDocumentCommand>(dto), ct)));

    /// Deletes an existing document and returns HTTP 204 without a response body.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }

    /// Returns the notes associated with an existing document.
    [HttpGet("{id:guid}/notes")]
    public async Task<ActionResult<IReadOnlyList<DocumentNoteDto>>> GetNotes(Guid id, CancellationToken ct) =>
        Ok((await service.GetNotesAsync(id, ct)).Select(mapper.Map<DocumentNoteDto>).ToList());

    /// Creates a note for an existing document and returns HTTP 201.
    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<DocumentNoteDto>> AddNote(Guid id, CreateNoteDto dto, CancellationToken ct)
    {
        var note = await service.AddNoteAsync(id, mapper.Map<CreateNoteCommand>(dto), ct);
        return CreatedAtAction(nameof(GetNotes), new { id }, mapper.Map<DocumentNoteDto>(note));
    }
}
