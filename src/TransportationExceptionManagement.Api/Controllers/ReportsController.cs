using Microsoft.AspNetCore.Mvc;
using TransportationExceptionManagement.Application.Reports;
using TransportationExceptionManagement.Application.Reports.Dtos;

namespace TransportationExceptionManagement.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType<SummaryReportDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SummaryReportDto>> GetSummary(
        CancellationToken cancellationToken) =>
        Ok(await reportService.GetSummaryAsync(cancellationToken));

    [HttpGet("sla")]
    [ProducesResponseType<SlaReportDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SlaReportDto>> GetSla(
        CancellationToken cancellationToken) =>
        Ok(await reportService.GetSlaAsync(cancellationToken));
}
