using TransportationExceptionManagement.Application.Reports.Dtos;

namespace TransportationExceptionManagement.Application.Reports;

public interface IReportService
{
    Task<SummaryReportDto> GetSummaryAsync(CancellationToken cancellationToken);

    Task<SlaReportDto> GetSlaAsync(CancellationToken cancellationToken);
}
