using System.Text;
using Microsoft.AspNetCore.Mvc;
using TransportationExceptionManagement.Application.Cases;
using TransportationExceptionManagement.Application.Cases.Dtos;
using TransportationExceptionManagement.Application.Cases.Models;
using TransportationExceptionManagement.Application.Common;
using TransportationExceptionManagement.Application.Exports;

namespace TransportationExceptionManagement.Api.Controllers;

[ApiController]
[Route("api/cases")]
public sealed class CasesController(
    ICaseService caseService,
    ICaseCsvExportService csvExportService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<CaseListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<CaseListItemDto>>> GetCases(
        [FromQuery] CaseQuery query,
        CancellationToken cancellationToken) =>
        Ok(await caseService.SearchAsync(query, cancellationToken));

    [HttpGet("export.csv")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportCases(
        [FromQuery] CaseFilter filter,
        CancellationToken cancellationToken)
    {
        var csv = await csvExportService.ExportAsync(filter, cancellationToken);
        return File(
            Encoding.UTF8.GetBytes(csv),
            "text/csv; charset=utf-8",
            "transportation-exception-cases.csv");
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<CaseDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDetailDto>> GetCase(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await caseService.GetAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<CaseDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CaseDetailDto>> CreateCase(
        [FromBody] CreateCaseRequest request,
        CancellationToken cancellationToken)
    {
        var created = await caseService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCase), new { id = created.Id }, created);
    }

    [HttpPatch("{id:int}/assignment")]
    [ProducesResponseType<CaseDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDetailDto>> AssignCase(
        int id,
        [FromBody] UpdateAssignmentRequest request,
        CancellationToken cancellationToken) =>
        Ok(await caseService.AssignAsync(id, request, cancellationToken));

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<CaseDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CaseDetailDto>> UpdateStatus(
        int id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await caseService.UpdateStatusAsync(id, request, cancellationToken));

    [HttpPost("{id:int}/notes")]
    [ProducesResponseType<CaseNoteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseNoteDto>> AddNote(
        int id,
        [FromBody] AddCaseNoteRequest request,
        CancellationToken cancellationToken)
    {
        var note = await caseService.AddNoteAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetCase), new { id }, note);
    }
}
