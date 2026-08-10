using TransportationExceptionManagement.Application.Cases.Models;

namespace TransportationExceptionManagement.Application.Exports;

public interface ICaseCsvExportService
{
    Task<string> ExportAsync(CaseFilter filter, CancellationToken cancellationToken);
}
